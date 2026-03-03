namespace FabMatch.Application.Common.Models;

/// <summary>Result of creating a payment intent.</summary>
/// <param name="PaymentIntentId">Gateway payment intent ID.</param>
/// <param name="ClientSecret">Client-side secret required to confirm the payment.</param>
/// <param name="AmountCents">Amount in smallest currency unit (cents).</param>
/// <param name="Currency">ISO 4217 currency code.</param>
public sealed record PaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    long AmountCents,
    string Currency);

/// <summary>Result of creating a subscription.</summary>
/// <param name="SubscriptionId">Gateway subscription ID.</param>
/// <param name="Status">Gateway subscription status (e.g. "active", "trialing").</param>
/// <param name="ClientSecret">Client secret for confirming payment if required.</param>
public sealed record SubscriptionResult(
    string SubscriptionId,
    string Status,
    string? ClientSecret);

/// <summary>Parsed webhook event from the payment gateway.</summary>
/// <param name="EventId">Unique event ID.</param>
/// <param name="EventType">Event type string (e.g. "payment_intent.succeeded").</param>
/// <param name="ResourceId">ID of the gateway resource that triggered the event.</param>
/// <param name="RawPayload">Full JSON payload for custom handling.</param>
public sealed record WebhookEvent(
    string EventId,
    string EventType,
    string ResourceId,
    string RawPayload);
