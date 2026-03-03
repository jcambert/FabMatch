using FabMatch.Domain.Entities;
using FabMatch.Infrastructure.Data;
using FabMatch.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace FabMatch.Tests.Infrastructure;

/// <summary>
/// Integration tests for repositories using an in-memory EF Core database.
/// For full PostgreSQL integration tests see the TestContainers-based tests.
/// </summary>
public sealed class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger _log;

    public RepositoryTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ClientRepository_AddAndGetById_ShouldReturnClient()
    {
        _log.Information("Testing ClientRepository add and get by ID");

        // Arrange
        var userId = Guid.NewGuid();
        var client = Client.Create(userId, "Test Corp", "Manufacturing", "Test company");
        var repo = new ClientRepository(_db);

        // Act
        await repo.AddAsync(client);
        await _db.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(client.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Test Corp");
        retrieved.UserId.Should().Be(userId);

        _log.Information("Client {Id} retrieved successfully", retrieved.Id);
    }

    [Fact]
    public async Task ClientRepository_SoftDelete_ShouldExcludeFromQuery()
    {
        // Arrange
        var client = Client.Create(Guid.NewGuid(), "Deleted Corp", "Industry", "Desc");
        var repo = new ClientRepository(_db);
        await repo.AddAsync(client);
        await _db.SaveChangesAsync();

        // Act
        client.Delete();
        repo.Update(client);
        await _db.SaveChangesAsync();

        // The global query filter should exclude deleted entities
        var all = await repo.GetAllAsync();

        // Assert
        all.Should().NotContain(c => c.Id == client.Id);
    }

    [Fact]
    public async Task NotificationRepository_MarkAllAsRead_ShouldUpdateAllUnread()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var repo = new NotificationRepository(_db);

        for (var i = 0; i < 3; i++)
        {
            var notif = Notification.Create(userId, Domain.Enums.NotificationType.NewMatch,
                $"Title {i}", $"Message {i}");
            await repo.AddAsync(notif);
        }
        await _db.SaveChangesAsync();

        // Act
        await repo.MarkAllAsReadAsync(userId);

        // Assert
        var unread = await repo.GetUnreadAsync(userId);
        unread.Should().BeEmpty();
    }

    [Fact]
    public async Task MatchRepository_ExistsAsync_ShouldReturnTrueForDuplicates()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repo = new MatchRepository(_db);

        var match = Match.Create(projectId, supplierId, 0.8);
        await repo.AddAsync(match);
        await _db.SaveChangesAsync();

        // Act
        var exists = await repo.ExistsAsync(projectId, supplierId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task MatchRepository_ExistsAsync_ShouldReturnFalseForNewPair()
    {
        // Arrange
        var repo = new MatchRepository(_db);

        // Act
        var exists = await repo.ExistsAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    public void Dispose() => _db.Dispose();
}
