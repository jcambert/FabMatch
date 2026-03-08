using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;

namespace FabMatch.Application.Common.Interfaces;

/// <summary>Read/write access to subscription plan configurations.</summary>
public interface ISubscriptionPlanRepository
{
    Task<IReadOnlyList<SubscriptionPlanConfig>> GetAllAsync(CancellationToken ct = default);
    Task<SubscriptionPlanConfig?> GetByTierAsync(SubscriptionTier tier, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
