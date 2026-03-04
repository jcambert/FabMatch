using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Matches.Commands.RunMatching;

/// <summary>
/// Handles <see cref="RunMatchingCommand"/>:
/// 1. Loads project and all suppliers with embeddings.
/// 2. Computes cosine-similarity or calls AI to score each (project, supplier) pair.
/// 3. Creates <see cref="Match"/> entities for the top-N suppliers above the threshold.
/// 4. Sends real-time notifications and emails to both clients and suppliers.
/// </summary>
public sealed class RunMatchingHandler : ICommandHandler<RunMatchingCommand, RunMatchingResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IAIService _ai;
    private readonly INotificationHubService _hub;
    private readonly IEmailService _email;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RunMatchingHandler> _logger;

    public RunMatchingHandler(
        IUnitOfWork uow,
        IAIService ai,
        INotificationHubService hub,
        IEmailService email,
        UserManager<ApplicationUser> userManager,
        ILogger<RunMatchingHandler> logger)
    {
        _uow = uow;
        _ai = ai;
        _hub = hub;
        _email = email;
        _userManager = userManager;
        _logger = logger;
    }

    public async ValueTask<RunMatchingResult> Handle(RunMatchingCommand cmd, CancellationToken ct)
    {
        // 1. Load project with details
        var project = await _uow.Projects.GetWithPlansAndMatchesAsync(cmd.ProjectId, ct)
            ?? throw new KeyNotFoundException($"Project {cmd.ProjectId} not found.");

        if (project.Status != ProjectStatus.Active)
            throw new InvalidOperationException("Matching is only available for active projects.");

        // 2. Build project description for AI scoring
        var projectDesc = BuildProjectDescription(project);

        // Ensure project embedding is up to date
        if (project.EmbeddingVector is null)
        {
            var emb = await _ai.GenerateEmbeddingAsync(projectDesc, ct);
            project.SetEmbedding(emb);
        }

        // 3. Load all suppliers with embeddings
        var suppliers = await _uow.Suppliers.GetAllWithEmbeddingsAsync(ct);
        _logger.LogInformation("Running matching for project {ProjectId} against {Count} suppliers",
            cmd.ProjectId, suppliers.Count);

        var scored = new List<(Supplier supplier, double score, string rationale)>();

        foreach (var supplier in suppliers)
        {
            // Skip if match already exists
            if (await _uow.Matches.ExistsAsync(project.Id, supplier.Id, ct))
                continue;

            var supplierDesc = BuildSupplierDescription(supplier);
            var scoreResult = await _ai.ComputeMatchScoreAsync(projectDesc, supplierDesc, ct);

            if (scoreResult.Score >= cmd.AffinityThreshold)
                scored.Add((supplier, scoreResult.Score, scoreResult.Rationale));
        }

        // 4. Take top-N
        var topMatches = scored
            .OrderByDescending(x => x.score)
            .Take(cmd.TopN)
            .ToList();

        var created = new List<MatchProposalDto>();
        foreach (var (supplier, score, rationale) in topMatches)
        {
            var match = Match.Create(project.Id, supplier.Id, score, rationale);
            await _uow.Matches.AddAsync(match, ct);

            // Notify client (SignalR)
            await _hub.SendToUserAsync(
                project.Client.UserId,
                NotificationType.NewMatch,
                "New Supplier Match",
                $"A new supplier '{supplier.CompanyName}' matched your project '{project.Title}'.",
                $"/projects/{project.Id}/matches",
                ct);

            // Notify supplier (SignalR)
            await _hub.SendToUserAsync(
                supplier.UserId,
                NotificationType.NewMatch,
                "New Project Match",
                $"Your profile matched project '{project.Title}'. Review it now.",
                $"/matches/{match.Id}",
                ct);

            // Email client
            var clientUser = await _userManager.FindByIdAsync(project.Client.UserId.ToString());
            if (clientUser?.Email is not null)
                await _email.SendMatchNotificationAsync(
                    clientUser.Email, clientUser.FullName,
                    project.Title, supplier.CompanyName,
                    $"/projects/{project.Id}/matches", ct);

            // Email supplier
            var supplierUser = await _userManager.FindByIdAsync(supplier.UserId.ToString());
            if (supplierUser?.Email is not null)
                await _email.SendMatchNotificationAsync(
                    supplierUser.Email, supplierUser.FullName,
                    project.Title, project.Client.CompanyName,
                    $"/matches/{match.Id}", ct);

            // Persist notification entities
            var clientNotif = Notification.Create(
                project.Client.UserId,
                NotificationType.NewMatch,
                "New Supplier Match",
                $"Supplier '{supplier.CompanyName}' matched your project.",
                $"/projects/{project.Id}/matches");
            await _uow.Notifications.AddAsync(clientNotif, ct);

            var supplierNotif = Notification.Create(
                supplier.UserId,
                NotificationType.NewMatch,
                "New Project Match",
                $"Project '{project.Title}' matched your profile.",
                $"/matches/{match.Id}");
            await _uow.Notifications.AddAsync(supplierNotif, ct);

            created.Add(new MatchProposalDto(match.Id, supplier.Id, supplier.CompanyName, score, rationale));
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Created {Count} matches for project {ProjectId}", created.Count, cmd.ProjectId);
        return new RunMatchingResult(created.Count, created);
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static string BuildProjectDescription(Project p)
    {
        var parts = new List<string> { p.Title, p.Description };
        if (p.RequiredProcesses.Count > 0)
            parts.Add($"Processes: {string.Join(", ", p.RequiredProcesses)}");
        if (p.Materials.Count > 0)
            parts.Add($"Materials: {string.Join(", ", p.Materials)}");
        return string.Join(". ", parts);
    }

    private static string BuildSupplierDescription(Supplier s)
    {
        var parts = new List<string> { s.CompanyName, s.Presentation };
        if (s.Materials.Count > 0)
            parts.Add($"Materials: {string.Join(", ", s.Materials)}");
        if (s.Certifications.Count > 0)
            parts.Add($"Certifications: {string.Join(", ", s.Certifications)}");
        var caps = s.ProductionCapabilities.Select(c => c.ProcessType).Distinct();
        if (caps.Any())
            parts.Add($"Processes: {string.Join(", ", caps)}");
        return string.Join(". ", parts);
    }
}
