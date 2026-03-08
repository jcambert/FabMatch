using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Admin.Commands.UpdateSubscriptionPlan;

public sealed record UpdateSubscriptionPlanCommand(
    SubscriptionTier Tier,
    decimal MonthlyPrice,
    int MaxProjects,
    int MaxAnalysesPerMonth,
    bool UnlimitedMatching,
    bool PrioritySupport,
    string[] Features) : ICommand<bool>;
