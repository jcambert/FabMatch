using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IMatchRepository"/>.</summary>
public sealed class MatchRepository : Repository<Match>, IMatchRepository
{
    public MatchRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Match>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => await Set
            .Include(m => m.Supplier)
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.AffinityScore)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Match>> GetBySupplierIdAsync(Guid supplierId, CancellationToken ct = default)
        => await Set
            .Include(m => m.Project)
                .ThenInclude(p => p.Client)
            .Where(m => m.SupplierId == supplierId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Match>> GetByStatusAsync(MatchStatus status, CancellationToken ct = default)
        => await Set
            .Include(m => m.Project)
            .Include(m => m.Supplier)
            .Where(m => m.Status == status)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid projectId, Guid supplierId, CancellationToken ct = default)
        => await Set.AnyAsync(m => m.ProjectId == projectId && m.SupplierId == supplierId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Match>> GetTopMatchesAsync(
        Guid projectId, int take = 5, CancellationToken ct = default)
        => await Set
            .Include(m => m.Supplier)
            .Where(m => m.ProjectId == projectId && m.Status == MatchStatus.Proposed)
            .OrderByDescending(m => m.AffinityScore)
            .Take(take)
            .ToListAsync(ct);
}
