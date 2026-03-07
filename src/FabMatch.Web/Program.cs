using FabMatch.Application;
using FabMatch.Domain.Entities;
using FabMatch.Infrastructure;
using FabMatch.Infrastructure.Data;
using FabMatch.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;

// ── Configure Serilog ─────────────────────────────────────────────────────────

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/fabmatch-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ── Serilog integration ───────────────────────────────────────────────────────
builder.Host.UseSerilog();

// ── Application & Infrastructure DI ──────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Blazor Server ─────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── MudBlazor ─────────────────────────────────────────────────────────────────
builder.Services.AddMudServices();

// ── SignalR (real-time notifications) ─────────────────────────────────────────
builder.Services.AddSignalR();

// ── Authentication (cookie-based via Identity) ────────────────────────────────
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/auth/login";
    opts.LogoutPath = "/auth/logout";
    opts.AccessDeniedPath = "/auth/access-denied";
    opts.Cookie.HttpOnly = true;
    opts.Cookie.SameSite = SameSiteMode.Strict;
    opts.ExpireTimeSpan = TimeSpan.FromDays(7);
    opts.SlidingExpiration = true;
});

// ── Authorization ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("ClientOnly", p => p.RequireRole("Client"));
    opts.AddPolicy("SupplierOnly", p => p.RequireRole("Supplier"));
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ── HTTP Context for Blazor ───────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Antiforgery ───────────────────────────────────────────────────────────────
builder.Services.AddAntiforgery();

// ── Controllers (for webhooks) ────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Web app services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<FabMatch.Web.Services.NotificationStateService>();

var app = builder.Build();

// ── Database migration on startup ────────────────────────────────────────────
await MigrateDatabaseAsync(app);

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ── Auth endpoints (must run on HTTP request pipeline, not Blazor circuit) ───
app.MapPost("/auth/login/execute", async (
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string? rememberMe,
    [FromForm] string? returnUrl,
    SignInManager<ApplicationUser> signInManager) =>
{
    // The form sends hidden="false" + checkbox="true" when checked → "false,true"
    var isPersistent = rememberMe != null && rememberMe.Contains("true");
    var result = await signInManager.PasswordSignInAsync(
        email, password, isPersistent, lockoutOnFailure: true);

    if (result.Succeeded)
    {
        var safeReturnUrl = IsSafeLocalReturnUrl(returnUrl) ? returnUrl! : "/dashboard";
        return Results.LocalRedirect(safeReturnUrl);
    }

    if (result.IsLockedOut)
    {
        return Results.LocalRedirect($"/auth/login?error=locked&returnUrl={Uri.EscapeDataString(returnUrl ?? string.Empty)}");
    }

    return Results.LocalRedirect($"/auth/login?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl ?? string.Empty)}");
}).DisableAntiforgery();

app.MapGet("/auth/logout/execute", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/auth/login");
});

// ── Blazor & SignalR endpoints ────────────────────────────────────────────────
app.MapRazorComponents<FabMatch.Web.App>()
    .AddInteractiveServerRenderMode();

app.MapHub<FabMatch.Infrastructure.Services.FabMatchNotificationHub>("/hubs/notifications");

app.MapControllers();

Log.Information("FabMatch application starting on {Environment}", app.Environment.EnvironmentName);
await app.RunAsync();

// ── Helpers ────────────────────────────────────────────────────────────────────

static async Task MigrateDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Running database migrations…");
        await db.Database.MigrateAsync();

        // Seed default roles
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in new[] { "Admin", "Client", "Supplier" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // Seed admin user from configuration
        await SeedAdminUserAsync(scope, logger);

        logger.LogInformation("Database migration complete.");

        // Ensure pg_trgm extension and GIN indexes for full-text search (idempotent)
        await ApplyTrgmIndexesAsync(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed.");
        throw;
    }
}

static async Task SeedAdminUserAsync(IServiceScope scope, ILogger<Program> logger)
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var email    = config["AdminSeed:Email"];
    var password = config["AdminSeed:Password"];
    var fullName = config["AdminSeed:FullName"] ?? "Admin";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return;

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (await userManager.FindByEmailAsync(email) is not null)
        return; // already exists

    var admin = new ApplicationUser
    {
        UserName = email,
        Email    = email,
        FullName = fullName,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(admin, password);
    if (!result.Succeeded)
    {
        logger.LogWarning("Admin seed failed: {Errors}",
            string.Join(", ", result.Errors.Select(e => e.Description)));
        return;
    }

    await userManager.AddToRoleAsync(admin, "Admin");
    logger.LogInformation("Admin user seeded: {Email}", email);
}

static async Task ApplyTrgmIndexesAsync(
    ApplicationDbContext db,
    ILogger<Program> logger)
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        await db.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS ix_suppliers_company_trgm
                ON ""Suppliers"" USING GIN (""CompanyName"" gin_trgm_ops);");

        await db.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS ix_suppliers_presentation_trgm
                ON ""Suppliers"" USING GIN (""Presentation"" gin_trgm_ops);");

        logger.LogInformation("pg_trgm GIN indexes verified.");
    }
    catch (Exception ex)
    {
        // Non-fatal: trgm indexes are a performance optimisation, not a correctness requirement.
        logger.LogWarning(ex, "Could not apply pg_trgm indexes (non-fatal).");
    }
}

static bool IsSafeLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return false;

    return returnUrl.StartsWith('/')
           && !returnUrl.StartsWith("//")
           && !returnUrl.StartsWith("/\\");
}

// Required for test discovery
public partial class Program { }
