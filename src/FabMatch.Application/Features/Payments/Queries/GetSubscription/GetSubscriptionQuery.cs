using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Payments.Queries.GetSubscription;

/// <summary>Returns the current subscription information for a given user.</summary>
public sealed record GetSubscriptionQuery(Guid UserId) : IQuery<SubscriptionInfoDto>;

/// <summary>Subscription information DTO.</summary>
/// <param name="CurrentTier">User's active subscription tier.</param>
/// <param name="StripeCustomerId">Stripe customer ID if set, null otherwise.</param>
/// <param name="HasPaymentMethod">True if the user has a saved payment method.</param>
public sealed record SubscriptionInfoDto(
    SubscriptionTier CurrentTier,
    string? StripeCustomerId,
    bool HasPaymentMethod);
