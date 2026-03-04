using Mediator;

namespace FabMatch.Application.Features.Payments.Commands.CancelSubscription;

/// <summary>
/// Cancels the user's current Stripe subscription at the end of the billing period.
/// Resets the user's tier to <see cref="FabMatch.Domain.Enums.SubscriptionTier.Free"/> immediately.
/// </summary>
public sealed record CancelSubscriptionCommand(
    Guid UserId,
    string SubscriptionId) : ICommand;
