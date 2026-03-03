namespace FabMatch.Domain.Enums;

/// <summary>States of a payment transaction.</summary>
public enum PaymentStatus
{
    /// <summary>Payment intent created, awaiting user action.</summary>
    Pending = 0,

    /// <summary>Payment successfully captured.</summary>
    Succeeded = 1,

    /// <summary>Payment failed or was declined.</summary>
    Failed = 2,

    /// <summary>Payment refunded.</summary>
    Refunded = 3,

    /// <summary>Payment cancelled before capture.</summary>
    Cancelled = 4
}
