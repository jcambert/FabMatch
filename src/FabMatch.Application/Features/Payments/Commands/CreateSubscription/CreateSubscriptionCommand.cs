using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Payments.Commands.CreateSubscription;

/// <summary>
/// Creates (or upgrades) a Stripe subscription for the given user.
/// The Stripe price ID is looked up from configuration based on the requested tier.
/// </summary>
public sealed record CreateSubscriptionCommand(
    Guid UserId,
    string UserEmail,
    string UserFullName,
    SubscriptionTier TargetTier) : ICommand<CreateSubscriptionResult>;

/// <summary>Result containing the Stripe subscription details needed by the frontend.</summary>
/// <param name="SubscriptionId">Stripe subscription ID.</param>
/// <param name="ClientSecret">Client secret for confirming payment (may be null if no payment step needed).</param>
/// <param name="Status">Stripe subscription status string.</param>
public sealed record CreateSubscriptionResult(
    string SubscriptionId,
    string? ClientSecret,
    string Status);
