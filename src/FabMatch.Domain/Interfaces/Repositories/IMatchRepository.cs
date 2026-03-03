using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for <see cref="Match"/> entities with domain-specific queries.
/// </summary>
public interface IMatchRepository : IRepository<Match>
{
    /// <summary>Returns all matches for a specific project, ordered by affinity descending.</summary>
    Task<IReadOnlyList<Match>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Returns all matches proposed to a specific supplier.</summary>
    Task<IReadOnlyList<Match>> GetBySupplierIdAsync(Guid supplierId, CancellationToken ct = default);

    /// <summary>Returns matches filtered by status.</summary>
    Task<IReadOnlyList<Match>> GetByStatusAsync(MatchStatus status, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a match already exists between a project and a supplier.
    /// Prevents duplicate proposals.
    /// </summary>
    Task<bool> ExistsAsync(Guid projectId, Guid supplierId, CancellationToken ct = default);

    /// <summary>Returns top-N pending matches for a given project.</summary>
    Task<IReadOnlyList<Match>> GetTopMatchesAsync(
        Guid projectId,
        int take = 5,
        CancellationToken ct = default);
}
