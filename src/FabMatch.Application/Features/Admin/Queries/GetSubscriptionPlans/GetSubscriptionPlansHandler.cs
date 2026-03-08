using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetSubscriptionPlans;

public sealed class GetSubscriptionPlansHandler
    : IQueryHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanConfigDto>>
{
    private readonly ISubscriptionPlanRepository _plans;

    public GetSubscriptionPlansHandler(ISubscriptionPlanRepository plans)
        => _plans = plans;

    public async ValueTask<IReadOnlyList<SubscriptionPlanConfigDto>> Handle(
        GetSubscriptionPlansQuery query, CancellationToken ct)
    {
        var configs = await _plans.GetAllAsync(ct);
        return configs
            .OrderBy(c => c.Tier)
            .Select(c => new SubscriptionPlanConfigDto(
                c.Tier,
                c.MonthlyPrice,
                c.MaxProjects,
                c.MaxAnalysesPerMonth,
                c.UnlimitedMatching,
                c.PrioritySupport,
                c.Features.ToArray(),
                c.UpdatedAt))
            .ToList();
    }
}
