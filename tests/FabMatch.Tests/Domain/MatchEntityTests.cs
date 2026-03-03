using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FluentAssertions;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace FabMatch.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Match"/> domain entity's state machine.
/// </summary>
public sealed class MatchEntityTests
{
    private readonly ILogger _log;

    public MatchEntityTests(ITestOutputHelper output)
    {
        _log = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public void Create_WithValidScore_ShouldReturnProposedMatch()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        // Act
        var match = Match.Create(projectId, supplierId, 0.87, "Good capability overlap.");

        // Assert
        match.Id.Should().NotBeEmpty();
        match.ProjectId.Should().Be(projectId);
        match.SupplierId.Should().Be(supplierId);
        match.AffinityScore.Should().Be(0.87);
        match.Status.Should().Be(MatchStatus.Proposed);
        match.ClientRespondedAt.Should().BeNull();

        _log.Information("Match {Id} created with score {Score}", match.Id, match.AffinityScore);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void Create_WithInvalidScore_ShouldThrow(double invalidScore)
    {
        var act = () => Match.Create(Guid.NewGuid(), Guid.NewGuid(), invalidScore);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AcceptByClient_ShouldTransitionToAcceptedByClient()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), 0.75);

        match.AcceptByClient();

        match.Status.Should().Be(MatchStatus.AcceptedByClient);
        match.ClientRespondedAt.Should().NotBeNull();
    }

    [Fact]
    public void RejectByClient_ShouldTransitionToRejectedByClient()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), 0.60);

        match.RejectByClient();

        match.Status.Should().Be(MatchStatus.RejectedByClient);
    }

    [Fact]
    public void AcceptBySupplier_AfterClientAcceptance_ShouldTransitionToAcceptedBySupplier()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), 0.80);
        match.AcceptByClient();

        match.AcceptBySupplier();

        match.Status.Should().Be(MatchStatus.AcceptedBySupplier);
        match.SupplierRespondedAt.Should().NotBeNull();
    }

    [Fact]
    public void AcceptBySupplier_WithoutClientAcceptance_ShouldThrow()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), 0.80);

        var act = match.AcceptBySupplier;

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*client acceptance*");
    }

    [Fact]
    public void Finalise_AfterMutualAcceptance_ShouldTransitionToFinalised()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), 0.90);
        match.AcceptByClient();
        match.AcceptBySupplier();

        match.Finalise();

        match.Status.Should().Be(MatchStatus.Finalised);
    }

    [Fact]
    public void Finalise_WithoutSupplierAcceptance_ShouldThrow()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), 0.70);
        match.AcceptByClient();

        var act = match.Finalise;

        act.Should().Throw<InvalidOperationException>();
    }
}
