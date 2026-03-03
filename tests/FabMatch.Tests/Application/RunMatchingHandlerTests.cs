using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using FabMatch.Application.Features.Matches.Commands.RunMatching;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using FabMatch.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace FabMatch.Tests.Application;

/// <summary>
/// Unit tests for <see cref="RunMatchingHandler"/>.
/// </summary>
public sealed class RunMatchingHandlerTests
{
    private readonly ILogger _log;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IProjectRepository> _projectRepoMock;
    private readonly Mock<ISupplierRepository> _supplierRepoMock;
    private readonly Mock<IMatchRepository> _matchRepoMock;
    private readonly Mock<INotificationRepository> _notifRepoMock;
    private readonly Mock<IAIService> _aiMock;
    private readonly Mock<INotificationHubService> _hubMock;

    public RunMatchingHandlerTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        _uowMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _supplierRepoMock = new Mock<ISupplierRepository>();
        _matchRepoMock = new Mock<IMatchRepository>();
        _notifRepoMock = new Mock<INotificationRepository>();
        _aiMock = new Mock<IAIService>();
        _hubMock = new Mock<INotificationHubService>();

        _uowMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _uowMock.Setup(u => u.Suppliers).Returns(_supplierRepoMock.Object);
        _uowMock.Setup(u => u.Matches).Returns(_matchRepoMock.Object);
        _uowMock.Setup(u => u.Notifications).Returns(_notifRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_WithActiveProjectAndSuppliers_ShouldCreateMatches()
    {
        _log.Information("Testing RunMatchingHandler with valid project and suppliers");

        // Arrange
        var clientUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "client@example.com"
        };
        var client = Client.Create(clientUser.Id, "Client Corp", "Automotive", "Desc");

        var project = Project.Create(client.Id, "Test Project", "Laser cut parts");
        // Set project to Active (it has no plans so we force status via reflection for test)
        typeof(Project).GetProperty("Status")!.SetValue(project, ProjectStatus.Active);
        typeof(Project).GetProperty("Client")!.SetValue(project, client);

        var supplierUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "supplier@example.com"
        };
        var supplier = Supplier.Create(supplierUser.Id, "Metal Works", "Expert sheet metal", "France");
        supplier.SetEmbedding([0.1f, 0.2f]);
        typeof(Supplier).GetProperty("UserId")!.SetValue(supplier, supplierUser.Id);

        _projectRepoMock
            .Setup(r => r.GetWithPlansAndMatchesAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _supplierRepoMock
            .Setup(r => r.GetAllWithEmbeddingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Supplier> { supplier });

        _matchRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _matchRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Match>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notifRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _aiMock
            .Setup(a => a.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([0.1f, 0.2f]);

        _aiMock
            .Setup(a => a.ComputeMatchScoreAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchScoreResult(0.85, "Excellent capability match."));

        _hubMock
            .Setup(h => h.SendToUserAsync(
                It.IsAny<Guid>(), It.IsAny<Domain.Enums.NotificationType>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RunMatchingHandler(
            _uowMock.Object, _aiMock.Object, _hubMock.Object,
            NullLogger<RunMatchingHandler>.Instance);

        var cmd = new RunMatchingCommand(project.Id, TopN: 5, AffinityThreshold: 0.55);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.MatchesCreated.Should().Be(1);
        result.Matches.Should().HaveCount(1);
        result.Matches[0].AffinityScore.Should().Be(0.85);
        result.Matches[0].SupplierName.Should().Be("Metal Works");

        _matchRepoMock.Verify(r => r.AddAsync(It.IsAny<Match>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _log.Information("Matching completed: {Count} matches created", result.MatchesCreated);
    }

    [Fact]
    public async Task Handle_WithInactiveProject_ShouldThrow()
    {
        // Arrange
        var project = Project.Create(Guid.NewGuid(), "Draft Project", "Desc");
        // Status is Draft by default

        _projectRepoMock
            .Setup(r => r.GetWithPlansAndMatchesAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new RunMatchingHandler(
            _uowMock.Object, _aiMock.Object, _hubMock.Object,
            NullLogger<RunMatchingHandler>.Instance);

        // Act & Assert
        var act = () => handler.Handle(new RunMatchingCommand(project.Id), CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active*");
    }
}
