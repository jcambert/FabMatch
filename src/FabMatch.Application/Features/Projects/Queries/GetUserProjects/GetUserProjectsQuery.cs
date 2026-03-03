using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Projects.Queries.GetUserProjects;

/// <summary>Query to retrieve all projects for a given client user.</summary>
public sealed record GetUserProjectsQuery(Guid ClientId, ProjectStatus? StatusFilter = null)
    : IQuery<IReadOnlyList<ProjectSummaryDto>>;

/// <summary>Lightweight project summary for list views.</summary>
public sealed record ProjectSummaryDto(
    Guid Id,
    string Title,
    string Description,
    ProjectStatus Status,
    bool IsPublic,
    int PlanCount,
    int MatchCount,
    DateTime? DeliveryDate,
    decimal? BudgetEstimate,
    string BudgetCurrency,
    DateTime CreatedAt);
