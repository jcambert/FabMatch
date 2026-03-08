using FabMatch.Application.Common.Models;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetSubscriptionPlans;

/// <summary>Returns the editable configuration for all subscription tiers.</summary>
public sealed record GetSubscriptionPlansQuery : IQuery<IReadOnlyList<SubscriptionPlanConfigDto>>;
