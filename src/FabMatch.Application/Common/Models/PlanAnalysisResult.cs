namespace FabMatch.Application.Common.Models;

/// <summary>
/// DTO returned by <see cref="Interfaces.IAIService.AnalysePlanAsync"/> containing
/// AI-extracted manufacturing metadata and cost estimates.
/// </summary>
public sealed record PlanAnalysisResult(
    IReadOnlyList<string> IdentifiedProcesses,
    IReadOnlyList<string> IdentifiedMaterials,
    decimal? EstimatedManufacturingCost,
    decimal? EstimatedSellingPrice,
    string Currency,
    int? EstimatedDurationDays,
    double ConfidenceScore,
    string Summary,
    string RawResponse);
