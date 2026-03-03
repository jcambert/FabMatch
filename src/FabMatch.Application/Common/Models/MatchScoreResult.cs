namespace FabMatch.Application.Common.Models;

/// <summary>
/// DTO returned by <see cref="Interfaces.IAIService.ComputeMatchScoreAsync"/>
/// containing the affinity score and a human-readable rationale.
/// </summary>
/// <param name="Score">Affinity score between 0 (no match) and 1 (perfect match).</param>
/// <param name="Rationale">Short explanation of why the score was assigned.</param>
public sealed record MatchScoreResult(double Score, string Rationale);
