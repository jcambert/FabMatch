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

    public CreateProjectHandlerTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        _uowMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _clientRepoMock = new Mock<IClientRepository>();

        _uowMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);
        _uowMock.Setup(u => u.Clients).Returns(_clientRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

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

        var handler = new CreateProjectHandler(_uowMock.Object, NullLogger<CreateProjectHandler>.Instance);

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

        var handler = new CreateProjectHandler(_uowMock.Object, NullLogger<CreateProjectHandler>.Instance);

        var cmd = new CreateProjectCommand(
            Guid.NewGuid(), "Test", "Description", false, null, null, "EUR", null, null);

        // Act & Assert
        var act = () => handler.Handle(cmd, CancellationToken.None).AsTask();
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
