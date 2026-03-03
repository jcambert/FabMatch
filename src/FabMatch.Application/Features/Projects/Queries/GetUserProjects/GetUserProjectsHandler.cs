using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Projects.Queries.GetUserProjects;

/// <summary>
/// Returns all projects belonging to a client, optionally filtered by status.
/// </summary>
public sealed class GetUserProjectsHandler
    : IQueryHandler<GetUserProjectsQuery, IReadOnlyList<ProjectSummaryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GetUserProjectsHandler> _logger;

    public GetUserProjectsHandler(IUnitOfWork uow, ILogger<GetUserProjectsHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async ValueTask<IReadOnlyList<ProjectSummaryDto>> Handle(
        GetUserProjectsQuery query, CancellationToken ct)
    {
        var projects = await _uow.Projects.GetByClientIdAsync(query.ClientId, ct);

        if (query.StatusFilter.HasValue)
            projects = projects.Where(p => p.Status == query.StatusFilter.Value).ToList();

        return projects.Select(p => new ProjectSummaryDto(
            p.Id,
            p.Title,
            p.Description,
            p.Status,
            p.IsPublic,
            p.Plans.Count,
            p.Matches.Count,
            p.DeliveryDate,
            p.BudgetEstimate,
            p.BudgetCurrency,
            p.CreatedAt)).ToList();
    }
}
