using FabMatch.Domain.Entities;
using FabMatch.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace FabMatch.Infrastructure.Data;

/// <summary>
/// Main EF Core DbContext for the FabMatch application.
/// Extends <see cref="IdentityDbContext"/> to include ASP.NET Core Identity tables.
/// All entity configurations are in <c>Configurations/</c> sub-folder.
/// </summary>
public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Domain DbSets ──────────────────────────────────────────────

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductionCapability> ProductionCapabilities => Set<ProductionCapability>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Analysis> Analyses => Set<Analysis>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Payment> Payments => Set<Payment>();

    // ── Model configuration ────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ──────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.OwnsOne(u => u.Address, a =>
            {
                a.Property(x => x.Street).HasMaxLength(200);
                a.Property(x => x.City).HasMaxLength(100);
                a.Property(x => x.PostalCode).HasMaxLength(20);
                a.Property(x => x.Country).HasMaxLength(100);
            });
        });

        // ── Client ───────────────────────────────────────────────
        builder.Entity<Client>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.User)
             .WithOne(u => u.ClientProfile)
             .HasForeignKey<Client>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(c => c.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(c => c.Industry).HasMaxLength(100);
            e.Property(c => c.Description).HasMaxLength(5000);
            e.HasQueryFilter(c => !c.IsDeleted);
        });

        // ── Supplier ─────────────────────────────────────────────
        builder.Entity<Supplier>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.User)
             .WithOne(u => u.SupplierProfile)
             .HasForeignKey<Supplier>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(s => s.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(s => s.Country).HasMaxLength(100);

            // Store list of strings as JSON arrays
            e.Property(s => s.Certifications)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
             .Metadata.SetValueComparer(StringListComparer());

            e.Property(s => s.Materials)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
             .Metadata.SetValueComparer(StringListComparer());

            // Embedding vector – stored as float[] JSON
            e.Property(s => s.EmbeddingVector)
             .HasConversion(
                 v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => v == null ? null : JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null))
             .HasColumnType("text");

            // MoneyAmount owned types
            e.OwnsOne(s => s.MinOrderValue, mo =>
            {
                mo.Property(x => x.Amount).HasColumnName("MinOrderAmount");
                mo.Property(x => x.Currency).HasColumnName("MinOrderCurrency").HasMaxLength(3);
            });
            e.OwnsOne(s => s.MaxOrderValue, mo =>
            {
                mo.Property(x => x.Amount).HasColumnName("MaxOrderAmount");
                mo.Property(x => x.Currency).HasColumnName("MaxOrderCurrency").HasMaxLength(3);
            });

            e.HasQueryFilter(s => !s.IsDeleted);
        });

        // ── ProductionCapability ──────────────────────────────────
        builder.Entity<ProductionCapability>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Supplier)
             .WithMany(s => s.ProductionCapabilities)
             .HasForeignKey(c => c.SupplierId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(c => c.ProcessType).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(c => !c.IsDeleted);
        });

        // ── Project ───────────────────────────────────────────────
        builder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Client)
             .WithMany(c => c.Projects)
             .HasForeignKey(p => p.ClientId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.Title).HasMaxLength(200).IsRequired();
            e.Property(p => p.BudgetCurrency).HasMaxLength(3);

            e.Property(p => p.RequiredProcesses)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
             .Metadata.SetValueComparer(StringListComparer());

            e.Property(p => p.Materials)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
             .Metadata.SetValueComparer(StringListComparer());

            e.Property(p => p.EmbeddingVector)
             .HasConversion(
                 v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => v == null ? null : JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null))
             .HasColumnType("text");

            e.HasQueryFilter(p => !p.IsDeleted);
        });

        // ── Plan ──────────────────────────────────────────────────
        builder.Entity<Plan>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Project)
             .WithMany(pr => pr.Plans)
             .HasForeignKey(p => p.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.OriginalFileName).HasMaxLength(500).IsRequired();
            e.Property(p => p.StorageKey).HasMaxLength(1000).IsRequired();
            e.HasQueryFilter(p => !p.IsDeleted);
        });

        // ── Analysis ──────────────────────────────────────────────
        builder.Entity<Analysis>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Plan)
             .WithMany(p => p.Analyses)
             .HasForeignKey(a => a.PlanId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(a => a.IdentifiedProcesses)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
             .Metadata.SetValueComparer(StringListComparer());

            e.Property(a => a.IdentifiedMaterials)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
             .Metadata.SetValueComparer(StringListComparer());

            e.HasQueryFilter(a => !a.IsDeleted);
        });

        // ── Match ─────────────────────────────────────────────────
        builder.Entity<Match>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.Project)
             .WithMany(p => p.Matches)
             .HasForeignKey(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Supplier)
             .WithMany(s => s.Matches)
             .HasForeignKey(m => m.SupplierId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(m => new { m.ProjectId, m.SupplierId }).IsUnique();
            e.HasQueryFilter(m => !m.IsDeleted);
        });

        // ── Notification ──────────────────────────────────────────
        builder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasOne(n => n.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(n => n.Title).HasMaxLength(200);
            e.Property(n => n.Message).HasMaxLength(2000);
            e.HasIndex(n => new { n.UserId, n.IsRead });
            e.HasQueryFilter(n => !n.IsDeleted);
        });

        // ── Payment ───────────────────────────────────────────────
        builder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User)
             .WithMany(u => u.Payments)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.OwnsOne(p => p.Amount, a =>
            {
                a.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                a.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
            });

            e.HasQueryFilter(p => !p.IsDeleted);
        });
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static ValueComparer<List<string>> StringListComparer() =>
        new(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
}
