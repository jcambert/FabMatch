using Mediator;

namespace FabMatch.Application.Features.Matches.Commands.RunMatching;

/// <summary>
/// Command to trigger AI-driven matchmaking for a specific project.
/// Compares the project embedding against all suppliers and creates match proposals
/// for the top-scoring suppliers above the affinity threshold.
/// </summary>
/// <param name="ProjectId">The project to run matching for.</param>
/// <param name="TopN">Maximum number of match proposals to create (default 5).</param>
/// <param name="AffinityThreshold">Minimum affinity score to propose a match (default 0.55).</param>
public sealed record RunMatchingCommand(
    Guid ProjectId,
    int TopN = 5,
    double AffinityThreshold = 0.55) : ICommand<RunMatchingResult>;

/// <summary>Summary of the matching run.</summary>
/// <param name="MatchesCreated">Number of new match proposals created.</param>
/// <param name="Matches">Details of each created match.</param>
public sealed record RunMatchingResult(
    int MatchesCreated,
    IReadOnlyList<MatchProposalDto> Matches);

/// <summary>Lightweight DTO for a single match proposal.</summary>
public sealed record MatchProposalDto(
    Guid MatchId,
    Guid SupplierId,
    string SupplierName,
    double AffinityScore,
    string? Rationale);
