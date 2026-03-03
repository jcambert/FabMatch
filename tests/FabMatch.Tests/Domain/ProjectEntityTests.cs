using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FluentAssertions;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace FabMatch.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Project"/> entity and its state machine.
/// </summary>
public sealed class ProjectEntityTests
{
    private readonly ILogger _log;

    public ProjectEntityTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateDraftProject()
    {
        var clientId = Guid.NewGuid();

        var project = Project.Create(clientId, "Chassis Bracket", "Laser cut steel bracket");

        project.Id.Should().NotBeEmpty();
        project.ClientId.Should().Be(clientId);
        project.Title.Should().Be("Chassis Bracket");
        project.Status.Should().Be(ProjectStatus.Draft);
        project.IsPublic.Should().BeFalse();
        project.Plans.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrow()
    {
        var act = () => Project.Create(Guid.NewGuid(), "", "Description");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Publish_WithNoPlans_ShouldThrow()
    {
        var project = Project.Create(Guid.NewGuid(), "Test", "Description");

        var act = project.Publish;

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*no plans*");
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        var project = Project.Create(Guid.NewGuid(), "Test", "Description");

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
        project.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetEmbedding_ShouldStoreVector()
    {
        var project = Project.Create(Guid.NewGuid(), "Test", "Description");
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        project.SetEmbedding(vector);

        project.EmbeddingVector.Should().Equal(vector);
        project.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldChangeAllFields()
    {
        var project = Project.Create(Guid.NewGuid(), "Original", "Old desc");

        project.Update(
            "Updated Title", "New description", true,
            DateTime.UtcNow.AddDays(30), 5000m, "EUR",
            ["Laser Cutting"], ["Steel"]);

        project.Title.Should().Be("Updated Title");
        project.IsPublic.Should().BeTrue();
        project.BudgetEstimate.Should().Be(5000m);
        project.RequiredProcesses.Should().Contain("Laser Cutting");
        project.Materials.Should().Contain("Steel");
    }
}
