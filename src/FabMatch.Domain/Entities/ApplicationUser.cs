using FabMatch.Domain.Enums;
using FabMatch.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FabMatch.Domain.Entities;

/// <summary>
/// Platform user that extends ASP.NET Core Identity's <see cref="IdentityUser{TKey}"/>.
/// A single user account may act as a <see cref="Client"/>, a <see cref="Supplier"/>, or both.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    // ── Basic profile ──────────────────────────────────────────────

    /// <summary>User's given name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>User's family name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Display name shown in the UI.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>URL of the user's avatar/profile picture.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Postal address of the user / company head-office.</summary>
    public Address? Address { get; set; }

    // ── Account meta ───────────────────────────────────────────────

    /// <summary>UTC timestamp of account creation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of last login.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Whether the account has completed on-boarding.</summary>
    public bool IsOnboarded { get; set; }

    // ── Subscription ───────────────────────────────────────────────

    /// <summary>Current subscription tier.</summary>
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;

    /// <summary>Stripe customer ID for payment operations.</summary>
    public string? StripeCustomerId { get; set; }

    // ── Navigation properties ──────────────────────────────────────

    /// <summary>Client profile linked to this user (null if not acting as client).</summary>
    public Client? ClientProfile { get; set; }

    /// <summary>Supplier profile linked to this user (null if not acting as supplier).</summary>
    public Supplier? SupplierProfile { get; set; }

    /// <summary>Notifications targeted at this user.</summary>
    public ICollection<Notification> Notifications { get; set; } = [];

    /// <summary>Payments made by this user.</summary>
    public ICollection<Payment> Payments { get; set; } = [];
}
