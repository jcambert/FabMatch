using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Plans.Commands.UploadPlan;

/// <summary>
/// Command to upload a technical plan (PNG, JPEG or PDF) to a project.
/// PDF files are automatically converted to PNG images before AI analysis.
/// </summary>
public sealed record UploadPlanCommand(
    Guid ProjectId,
    string OriginalFileName,
    byte[] FileContent,
    PlanFormat Format,
    string? Revision,
    int? DurationDays,
    int? Quantity,
    string? Notes,
    bool TriggerAnalysis = true) : ICommand<UploadPlanResult>;

/// <summary>Result containing the created plan ID and analysis status.</summary>
public sealed record UploadPlanResult(Guid PlanId, string StorageKey, bool AnalysisQueued);
