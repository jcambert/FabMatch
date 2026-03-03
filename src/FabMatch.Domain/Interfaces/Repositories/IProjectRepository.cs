using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for <see cref="Project"/> entities with domain-specific queries.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>Returns all projects belonging to a specific client.</summary>
    Task<IReadOnlyList<Project>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);

    /// <summary>Returns all public active projects (visible to suppliers for browsing).</summary>
    Task<IReadOnlyList<Project>> GetPublicActiveProjectsAsync(
        int skip = 0,
        int take = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the project with plans and matches eagerly loaded.
    /// </summary>
    Task<Project?> GetWithPlansAndMatchesAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Returns projects that have an embedding and are in the given status.</summary>
    Task<IReadOnlyList<Project>> GetProjectsForMatchingAsync(
        ProjectStatus status = ProjectStatus.Active,
        CancellationToken ct = default);
}
