using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IAnalysisRepository"/>.</summary>
public sealed class AnalysisRepository : Repository<Analysis>, IAnalysisRepository
{
    public AnalysisRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Analysis>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
        => await Set
            .Where(a => a.PlanId == planId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
}
