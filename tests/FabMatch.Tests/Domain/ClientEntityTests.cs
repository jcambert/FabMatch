using FabMatch.Domain.Entities;
using FluentAssertions;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace FabMatch.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Client"/> domain entity.
/// </summary>
public sealed class ClientEntityTests
{
    private readonly ILogger _log;

    public ClientEntityTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output, LogEventLevel.Debug)
            .CreateLogger();
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnClientWithNewId()
    {
        _log.Information("Testing Client.Create with valid data");

        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var client = Client.Create(userId, "ACME Corp", "Aerospace", "Tier-1 supplier");

        // Assert
        client.Id.Should().NotBeEmpty();
        client.UserId.Should().Be(userId);
        client.CompanyName.Should().Be("ACME Corp");
        client.Industry.Should().Be("Aerospace");
        client.IsDeleted.Should().BeFalse();
        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _log.Information("Client created with ID {Id}", client.Id);
    }

    [Fact]
    public void Create_WithEmptyCompanyName_ShouldThrowArgumentException()
    {
        _log.Information("Testing Client.Create with empty company name");

        // Act & Assert
        var act = () => Client.Create(Guid.NewGuid(), "", "Industry", "Desc");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ShouldChangePropertiesAndSetUpdatedAt()
    {
        // Arrange
        var client = Client.Create(Guid.NewGuid(), "Old Name", "Old Industry", "Old Desc");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        client.Update("New Name", "New Industry", "New Desc", "https://example.com", "51-200");

        // Assert
        client.CompanyName.Should().Be("New Name");
        client.Industry.Should().Be("New Industry");
        client.WebsiteUrl.Should().Be("https://example.com");
        client.UpdatedAt.Should().NotBeNull();
        client.UpdatedAt!.Value.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var client = Client.Create(Guid.NewGuid(), "ACME", "Industry", "Desc");

        // Act
        client.Delete();

        // Assert
        client.IsDeleted.Should().BeTrue();
        client.UpdatedAt.Should().NotBeNull();
    }
}
