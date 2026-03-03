using FabMatch.Domain.Entities;
using FabMatch.Infrastructure.Data;
using FabMatch.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace FabMatch.Tests.Infrastructure;

/// <summary>
/// Integration tests using Testcontainers to spin up a real PostgreSQL instance.
/// These tests are slower but validate actual SQL generation and constraints.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PostgreSqlIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly ILogger _log;
    private ApplicationDbContext _db = null!;

    public PostgreSqlIntegrationTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        _postgres = new PostgreSqlBuilder()
            .WithDatabase("fabmatch_test")
            .WithUsername("fabmatch")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _db = new ApplicationDbContext(options);
        await _db.Database.EnsureCreatedAsync();

        _log.Information("PostgreSQL container started and schema created");
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Client_CanBePersistedAndRetrievedFromPostgreSQL()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await AddUserAsync(userId, "client-postgres@example.com");

        var client = Client.Create(userId, "PostgreSQL Corp", "Technology", "A real DB test");
        var repo = new ClientRepository(_db);

        // Act
        await repo.AddAsync(client);
        await _db.SaveChangesAsync();

        _db.ChangeTracker.Clear(); // force reload from DB

        var retrieved = await repo.GetByIdAsync(client.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("PostgreSQL Corp");
        retrieved.UserId.Should().Be(userId);

        _log.Information("PostgreSQL persistence test passed for client {Id}", client.Id);
    }

    [Fact]
    public async Task Supplier_WithJsonLists_CanBePersistedAndRetrieved()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await AddUserAsync(userId, "supplier-postgres@example.com");

        var supplier = Supplier.Create(userId, "Steel Masters", "Expert in steel", "Germany");
        supplier.Update(
            "Steel Masters", "Expert in steel", "Germany",
            "https://steelmasters.de", 1990, "51-200",
            ["ISO 9001", "EN 15085"], ["S235JR", "S355"],
            500m, null, null);

        var repo = new SupplierRepository(_db);

        // Act
        await repo.AddAsync(supplier);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var retrieved = await repo.GetWithCapabilitiesAsync(supplier.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Certifications.Should().Contain("ISO 9001");
        retrieved.Materials.Should().Contain("S235JR");
    }

    private async Task AddUserAsync(Guid userId, string email)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
}
