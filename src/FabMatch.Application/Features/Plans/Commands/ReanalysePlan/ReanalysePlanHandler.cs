using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Plans.Commands.ReanalysePlan;

/// <summary>
/// Loads the stored plan image, sends it to the AI service, and persists a new
/// <see cref="Analysis"/> record. Also notifies the project owner via SignalR.
/// </summary>
public sealed class ReanalysePlanHandler : ICommandHandler<ReanalysePlanCommand, ReanalysePlanResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IAIService _ai;
    private readonly INotificationHubService _hub;
    private readonly ITierPolicyService _tierPolicy;
    private readonly ILogger<ReanalysePlanHandler> _logger;

    public ReanalysePlanHandler(
        IUnitOfWork uow,
        IFileStorageService storage,
        IAIService ai,
        INotificationHubService hub,
        ITierPolicyService tierPolicy,
        ILogger<ReanalysePlanHandler> logger)
    {
        _uow = uow;
        _storage = storage;
        _ai = ai;
        _hub = hub;
        _tierPolicy = tierPolicy;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<ReanalysePlanResult> Handle(ReanalysePlanCommand cmd, CancellationToken ct)
    {
        // 1. Load the plan and its project context
        var plan = await _uow.Plans.GetByIdAsync(cmd.PlanId, ct)
            ?? throw new KeyNotFoundException($"Plan {cmd.PlanId} not found.");

        var project = await _uow.Projects.GetWithPlansAndMatchesAsync(plan.ProjectId, ct)
            ?? throw new KeyNotFoundException($"Project {plan.ProjectId} not found.");

        // ── Tier limit check ──────────────────────────────────────────────────
        var (allowed, reason) = await _tierPolicy.CanRunAnalysisAsync(project.ClientId, ct);
        if (!allowed)
            throw new InvalidOperationException(reason);

        // 2. Retrieve the stored image bytes
        var imageStream = await _storage.GetStreamAsync(plan.StorageKey, ct);
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, ct);
        var imageBytes = ms.ToArray();

        // 3. Send to AI for analysis
        var currency = string.IsNullOrWhiteSpace(cmd.Currency) ? project.BudgetCurrency : cmd.Currency;
        var result = await _ai.AnalysePlanAsync(imageBytes, "image/png", currency, ct);

        // 4. Persist the new analysis
        var analysis = Analysis.Create(
            cmd.PlanId,
            result.IdentifiedProcesses.ToList(),
            result.IdentifiedMaterials.ToList(),
            result.EstimatedManufacturingCost,
            result.EstimatedSellingPrice,
            result.Currency,
            result.EstimatedDurationDays,
            result.ConfidenceScore,
            result.RawResponse,
            result.Summary,
            _ai.ProviderName);

        await _uow.Analyses.AddAsync(analysis, ct);
        plan.MarkAnalysed(analysis);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Re-analysis complete for plan {PlanId}. AnalysisId={AnalysisId}",
            cmd.PlanId, analysis.Id);

        // 5. Notify the project owner
        var ownerUserId = project.Client?.UserId ?? Guid.Empty;
        if (ownerUserId != Guid.Empty)
        {
            await _hub.SendToUserAsync(
                ownerUserId,
                Domain.Enums.NotificationType.AnalysisComplete,
                "Re-analysis Complete",
                $"Updated AI analysis for plan in project '{project.Title}' is ready.",
                $"/projects/{project.Id}",
                ct);
        }

        return new ReanalysePlanResult(analysis.Id, result.Summary, result.ConfidenceScore);
    }
}
