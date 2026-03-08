using FabMatch.Domain.Enums;

namespace FabMatch.Domain.Entities;

/// <summary>
/// Admin-editable configuration for a subscription tier.
/// One row per tier, seeded on first migration.
/// </summary>
public sealed class SubscriptionPlanConfig
{
    /// <summary>Primary key — one config per tier.</summary>
    public SubscriptionTier Tier { get; set; }

    /// <summary>Monthly price in euros. 0 = free.</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>Maximum number of active projects. -1 = unlimited.</summary>
    public int MaxProjects { get; set; }

    /// <summary>Maximum AI analyses per calendar month. -1 = unlimited.</summary>
    public int MaxAnalysesPerMonth { get; set; }

    public bool UnlimitedMatching { get; set; }

    public bool PrioritySupport { get; set; }

    /// <summary>Feature bullet-points shown on the pricing page.</summary>
    public List<string> Features { get; set; } = [];

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
