using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data;

public sealed class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly ApplicationDbContext _db;

    public SubscriptionPlanRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SubscriptionPlanConfig>> GetAllAsync(CancellationToken ct = default)
        => await _db.SubscriptionPlanConfigs.AsNoTracking().ToListAsync(ct);

    public async Task<SubscriptionPlanConfig?> GetByTierAsync(SubscriptionTier tier, CancellationToken ct = default)
        => await _db.SubscriptionPlanConfigs.FirstOrDefaultAsync(p => p.Tier == tier, ct);

    public Task SaveAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
