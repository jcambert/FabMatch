using FabMatch.Domain.Enums;

namespace FabMatch.Application.Common.Models;

public sealed record SubscriptionPlanConfigDto(
    SubscriptionTier Tier,
    decimal MonthlyPrice,
    int MaxProjects,
    int MaxAnalysesPerMonth,
    bool UnlimitedMatching,
    bool PrioritySupport,
    string[] Features,
    DateTime UpdatedAt);
