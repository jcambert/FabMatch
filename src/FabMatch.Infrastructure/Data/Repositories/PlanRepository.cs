using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IPlanRepository"/>.</summary>
public sealed class PlanRepository : Repository<Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Plan>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => await Set
            .Include(p => p.LatestAnalysis)
            .Where(p => p.ProjectId == projectId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<Plan?> GetWithAnalysisAsync(Guid planId, CancellationToken ct = default)
        => await Set
            .Include(p => p.Analyses)
            .FirstOrDefaultAsync(p => p.Id == planId, ct);
}
