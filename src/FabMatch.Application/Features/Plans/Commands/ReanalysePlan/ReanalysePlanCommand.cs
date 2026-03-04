using Mediator;

namespace FabMatch.Application.Features.Plans.Commands.ReanalysePlan;

/// <summary>
/// Triggers a fresh AI analysis for an existing plan.
/// The plan image is re-read from storage and sent to the AI provider.
/// Results are stored as a new <see cref="FabMatch.Domain.Entities.Analysis"/> record.
/// </summary>
/// <param name="PlanId">ID of the plan to re-analyse.</param>
/// <param name="Currency">Currency code used for cost estimates (e.g. "EUR").</param>
public sealed record ReanalysePlanCommand(Guid PlanId, string Currency = "EUR") : ICommand<ReanalysePlanResult>;

/// <summary>Result of the re-analysis operation.</summary>
/// <param name="AnalysisId">ID of the newly created analysis record.</param>
/// <param name="Summary">Short AI-generated summary.</param>
/// <param name="ConfidenceScore">Confidence score in the range [0, 1].</param>
public sealed record ReanalysePlanResult(Guid AnalysisId, string Summary, double ConfidenceScore);
