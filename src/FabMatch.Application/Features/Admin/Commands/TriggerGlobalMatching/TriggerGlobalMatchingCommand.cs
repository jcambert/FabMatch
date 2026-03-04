using Mediator;

namespace FabMatch.Application.Features.Admin.Commands.TriggerGlobalMatching;

/// <summary>
/// Admin command to run AI matchmaking for ALL active projects that have an embedding.
/// Returns the total number of new match proposals created across all projects.
/// </summary>
public sealed record TriggerGlobalMatchingCommand(int TopNPerProject = 5, double AffinityThreshold = 0.55)
    : ICommand<TriggerGlobalMatchingResult>;

/// <summary>Summary of the global matching run.</summary>
public sealed record TriggerGlobalMatchingResult(
    int ProjectsProcessed,
    int TotalMatchesCreated,
    IReadOnlyList<string> Errors);
