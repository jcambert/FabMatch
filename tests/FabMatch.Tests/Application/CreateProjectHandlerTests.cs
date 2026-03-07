using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Features.Projects.Commands.CreateProject;
using FabMatch.Domain.Entities;
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
/// Unit tests for <see cref="CreateProjectHandler"/>.
/// </summary>
public sealed class CreateProjectHandlerTests
{
    private readonly ILogger _log;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IProjectRepository> _projectRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<ITierPolicyService> _tierPolicyMock;

    public CreateProjectHandlerTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        _uowMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _tierPolicyMock = new Mock<ITierPolicyService>();

        _uowMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _uowMock.Setup(u => u.Clients).Returns(_clientRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // By default allow project creation
        _tierPolicyMock
            .Setup(t => t.CanCreateProjectAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null));
    }

    private CreateProjectHandler BuildHandler() =>
        new(_uowMock.Object, _tierPolicyMock.Object, NullLogger<CreateProjectHandler>.Instance);

    [Fact]
    public async Task Handle_WithValidClientAndCommand_ShouldCreateProject()
    {
        _log.Information("Testing successful project creation");

        // Arrange
        var clientId = Guid.NewGuid();
        var client = Client.Create(Guid.NewGuid(), "ACME", "Aerospace", "Description");

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _projectRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = BuildHandler();

        var cmd = new CreateProjectCommand(
            clientId, "Bracket Assembly", "Laser-cut steel bracket", false,
            null, 1000m, "EUR", ["Laser Cutting"], ["S235JR"]);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Bracket Assembly");
        result.ProjectId.Should().NotBeEmpty();

        _projectRepoMock.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _log.Information("Project {Id} created successfully", result.ProjectId);
    }

    [Fact]
    public async Task Handle_WithNonExistentClient_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _clientRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = BuildHandler();

        var cmd = new CreateProjectCommand(
            Guid.NewGuid(), "Test", "Description", false, null, null, "EUR", null, null);

        // Act & Assert
        var act = () => handler.Handle(cmd, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTierLimitExceeded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = Client.Create(Guid.NewGuid(), "ACME", "Aerospace", "Description");

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _tierPolicyMock
            .Setup(t => t.CanCreateProjectAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Limite de projets atteinte pour votre plan."));

        var handler = BuildHandler();

        var cmd = new CreateProjectCommand(
            clientId, "Test", "Description", false, null, null, "EUR", null, null);

        // Act & Assert
        var act = () => handler.Handle(cmd, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Limite*");
    }
}
