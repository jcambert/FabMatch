using FabMatch.Application.Common.Models;
using FabMatch.Domain.ValueObjects;

namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Abstraction over a payment gateway (Stripe, Braintree, etc.).
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates or retrieves a gateway customer record for the user.
    /// </summary>
    /// <param name="userId">Platform user ID.</param>
    /// <param name="email">User email for the gateway record.</param>
    /// <param name="fullName">User display name.</param>
    /// <returns>Gateway customer ID (e.g. Stripe customer ID "cus_…").</returns>
    Task<string> EnsureCustomerAsync(Guid userId, string email, string fullName, CancellationToken ct = default);

    /// <summary>
    /// Creates a payment intent / session for a one-off charge.
    /// </summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        string customerId,
        MoneyAmount amount,
        string description,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a subscription for the given price ID.
    /// </summary>
    Task<SubscriptionResult> CreateSubscriptionAsync(
        string customerId,
        string priceId,
        CancellationToken ct = default);

    /// <summary>
    /// Cancels a subscription at period end.
    /// </summary>
    Task CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Verifies a webhook event signature and returns the event payload.
    /// </summary>
    Task<WebhookEvent> VerifyWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default);
}
