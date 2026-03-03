using FabMatch.Domain.Common;
using FabMatch.Domain.Enums;
using FabMatch.Domain.ValueObjects;

namespace FabMatch.Domain.Entities;

/// <summary>
/// Records a payment transaction made by an <see cref="ApplicationUser"/>.
/// Covers subscription purchases and per-project service fees.
/// </summary>
public sealed class Payment : BaseEntity
{
    // ── Relations ──────────────────────────────────────────────────

    /// <summary>FK to the user who initiated the payment.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Navigation to the user.</summary>
    public ApplicationUser User { get; private set; } = null!;

    // ── Payment details ────────────────────────────────────────────

    /// <summary>Description shown on the user's invoice.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Amount charged.</summary>
    public MoneyAmount Amount { get; private set; } = MoneyAmount.Zero();

    /// <summary>Current payment status.</summary>
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    // ── Stripe references ──────────────────────────────────────────

    /// <summary>Stripe PaymentIntent ID.</summary>
    public string? StripePaymentIntentId { get; private set; }

    /// <summary>Stripe Invoice ID (for subscription payments).</summary>
    public string? StripeInvoiceId { get; private set; }

    // ── Factory / transitions ──────────────────────────────────────

    /// <summary>Creates a pending payment record.</summary>
    public static Payment Create(
        Guid userId,
        string description,
        MoneyAmount amount,
        string? stripePaymentIntentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        return new Payment
        {
            UserId = userId,
            Description = description,
            Amount = amount,
            StripePaymentIntentId = stripePaymentIntentId
        };
    }

    /// <summary>Marks the payment as successfully captured.</summary>
    public void Succeed() { Status = PaymentStatus.Succeeded; Touch(); }

    /// <summary>Marks the payment as failed.</summary>
    public void Fail() { Status = PaymentStatus.Failed; Touch(); }

    /// <summary>Marks the payment as refunded.</summary>
    public void Refund() { Status = PaymentStatus.Refunded; Touch(); }

    /// <summary>Marks the payment as cancelled.</summary>
    public void Cancel() { Status = PaymentStatus.Cancelled; Touch(); }
}
