using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces;
using FabMatch.Infrastructure.Data;
using FabMatch.Infrastructure.Services;
using FabMatch.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FabMatch.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services with the DI container.
/// Call in <c>Program.cs</c>: <c>builder.Services.AddInfrastructure(config);</c>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL / EF Core ───────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        // ── ASP.NET Core Identity ──────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opts =>
        {
            opts.Password.RequireDigit = true;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireUppercase = true;
            opts.Password.RequireNonAlphanumeric = false;
            opts.User.RequireUniqueEmail = true;
            opts.SignIn.RequireConfirmedAccount = false; // Set true in production
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ── Unit of Work ───────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── File Storage ───────────────────────────────────────────────────────
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.Section));
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // ── PDF Converter ──────────────────────────────────────────────────────
        services.AddScoped<IPdfConverterService, PdfConverterService>();

        // ── AI Service (selectable via config) ────────────────────────────────
        var aiProvider = configuration["AI:Provider"] ?? "OpenAI";
        if (aiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.Section));
            services.AddHttpClient<OllamaAIService>(client =>
            {
                var baseUrl = configuration[$"{OllamaOptions.Section}:BaseUrl"] ?? "http://localhost:11434";
                client.BaseAddress = new Uri(baseUrl);
            });
            services.AddScoped<IAIService, OllamaAIService>();
        }
        else
        {
            services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.Section));
            services.AddScoped<IAIService, OpenAIService>();
        }

        // ── Payment Service ────────────────────────────────────────────────────
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.Section));
        services.AddScoped<IPaymentService, StripePaymentService>();

        // ── SignalR Notification Hub Service ───────────────────────────────────
        services.AddScoped<INotificationHubService, NotificationHubService>();

        // ── Email Service (MailKit SMTP) ───────────────────────────────────────
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.Section));
        services.AddScoped<IEmailService, MailKitEmailService>();

        // ── Tier Policy (subscription limit enforcement) ───────────────────────
        services.AddScoped<ITierPolicyService, TierPolicyService>();

        return services;
    }
}
