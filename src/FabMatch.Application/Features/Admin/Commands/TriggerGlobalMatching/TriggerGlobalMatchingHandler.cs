using FabMatch.Application.Features.Matches.Commands.RunMatching;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Admin.Commands.TriggerGlobalMatching;

/// <summary>
/// Iterates over all active projects with embeddings and runs AI matching for each.
/// Errors per-project are collected and returned rather than aborting the whole run.
/// </summary>
public sealed class TriggerGlobalMatchingHandler
    : ICommandHandler<TriggerGlobalMatchingCommand, TriggerGlobalMatchingResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IMediator _mediator;
    private readonly ILogger<TriggerGlobalMatchingHandler> _logger;

    public TriggerGlobalMatchingHandler(
        IUnitOfWork uow, IMediator mediator, ILogger<TriggerGlobalMatchingHandler> logger)
    {
        _uow = uow;
        _mediator = mediator;
        _logger = logger;
    }

    public async ValueTask<TriggerGlobalMatchingResult> Handle(
        TriggerGlobalMatchingCommand cmd, CancellationToken ct)
    {
        var projects = await _uow.Projects.GetProjectsForMatchingAsync(ProjectStatus.Active, ct);
        _logger.LogInformation("Global matching started for {Count} projects", projects.Count);

        int totalMatches = 0;
        var errors = new List<string>();

        foreach (var project in projects)
        {
            try
            {
                var result = await _mediator.Send(
                    new RunMatchingCommand(project.Id, cmd.TopNPerProject, cmd.AffinityThreshold), ct);
                totalMatches += result.MatchesCreated;
                _logger.LogInformation(
                    "Project {Id}: {Matches} matches created", project.Id, result.MatchesCreated);
            }
            catch (Exception ex)
            {
                var msg = $"Project {project.Id} ({project.Title}): {ex.Message}";
                errors.Add(msg);
                _logger.LogError(ex, "Matching failed for project {Id}", project.Id);
            }
        }

        return new TriggerGlobalMatchingResult(projects.Count, totalMatches, errors);
    }
}
