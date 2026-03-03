using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Plans.Commands.UploadPlan;

/// <summary>
/// Handles <see cref="UploadPlanCommand"/>:
/// 1. Converts PDF to PNG if necessary.
/// 2. Stores the file via <see cref="IFileStorageService"/>.
/// 3. Creates the <see cref="Plan"/> entity.
/// 4. Optionally triggers AI analysis.
/// </summary>
public sealed class UploadPlanHandler : ICommandHandler<UploadPlanCommand, UploadPlanResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IPdfConverterService _pdfConverter;
    private readonly IAIService _ai;
    private readonly INotificationHubService _hub;
    private readonly ILogger<UploadPlanHandler> _logger;

    public UploadPlanHandler(
        IUnitOfWork uow,
        IFileStorageService storage,
        IPdfConverterService pdfConverter,
        IAIService ai,
        INotificationHubService hub,
        ILogger<UploadPlanHandler> logger)
    {
        _uow = uow;
        _storage = storage;
        _pdfConverter = pdfConverter;
        _ai = ai;
        _hub = hub;
        _logger = logger;
    }

    public async ValueTask<UploadPlanResult> Handle(UploadPlanCommand cmd, CancellationToken ct)
    {
        var project = await _uow.Projects.GetWithPlansAndMatchesAsync(cmd.ProjectId, ct)
            ?? throw new KeyNotFoundException($"Project {cmd.ProjectId} not found.");

        // 1. Convert PDF → PNG (take first page for storage key; all pages analysed below)
        byte[] storedBytes = cmd.FileContent;
        PlanFormat storedFormat = cmd.Format;
        string storedMime = cmd.Format == PlanFormat.Pdf ? "image/png" : ContentType(cmd.Format);

        if (cmd.Format == PlanFormat.Pdf)
        {
            var pages = await _pdfConverter.ConvertToPngAsync(cmd.FileContent, ct: ct);
            storedBytes = pages[0]; // Store first page as the plan image
            storedFormat = PlanFormat.Png;
        }

        // 2. Store the file
        var fileName = storedFormat == PlanFormat.Png
            ? Path.ChangeExtension(cmd.OriginalFileName, ".png")
            : cmd.OriginalFileName;

        using var ms = new MemoryStream(storedBytes);
        var storageKey = await _storage.SaveAsync(ms, fileName, "plans", ct);

        // 3. Create Plan entity
        var plan = Plan.Create(
            cmd.ProjectId,
            cmd.OriginalFileName,
            storedFormat,
            storageKey,
            storedBytes.Length,
            cmd.Revision,
            cmd.DurationDays,
            cmd.Quantity,
            cmd.Notes);

        await _uow.Plans.AddAsync(plan, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Plan {PlanId} uploaded for project {ProjectId}", plan.Id, cmd.ProjectId);

        // 4. Trigger AI analysis asynchronously if requested
        bool analysisQueued = false;
        if (cmd.TriggerAnalysis)
        {
            _ = AnalysePlanInBackgroundAsync(plan.Id, storedBytes, storedMime, project, ct);
            analysisQueued = true;
        }

        return new UploadPlanResult(plan.Id, storageKey, analysisQueued);
    }

    /// <summary>Runs AI analysis in a fire-and-forget task (background processing).</summary>
    private async Task AnalysePlanInBackgroundAsync(
        Guid planId,
        byte[] imageBytes,
        string mimeType,
        Domain.Entities.Project project,
        CancellationToken ct)
    {
        try
        {
            var result = await _ai.AnalysePlanAsync(imageBytes, mimeType, project.BudgetCurrency, ct);

            var analysis = Analysis.Create(
                planId,
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

            var plan = await _uow.Plans.GetByIdAsync(planId, ct);
            plan?.MarkAnalysed(analysis);

            await _uow.SaveChangesAsync(ct);

            // Notify project owner
            var clientUser = project.Client?.UserId ?? Guid.Empty;
            if (clientUser != Guid.Empty)
            {
                await _hub.SendToUserAsync(
                    clientUser,
                    Domain.Enums.NotificationType.AnalysisComplete,
                    "Analysis Complete",
                    $"AI analysis for plan in project '{project.Title}' is ready.",
                    $"/projects/{project.Id}",
                    ct);
            }

            _logger.LogInformation("AI analysis completed for plan {PlanId}", planId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI analysis failed for plan {PlanId}", planId);
        }
    }

    private static string ContentType(PlanFormat fmt) => fmt switch
    {
        PlanFormat.Png => "image/png",
        PlanFormat.Jpeg => "image/jpeg",
        _ => "application/octet-stream"
    };
}
