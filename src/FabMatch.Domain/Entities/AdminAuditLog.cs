namespace FabMatch.Domain.Entities;

/// <summary>
/// Immutable record of an admin action performed on the platform.
/// Audit logs are never soft-deleted.
/// </summary>
public sealed class AdminAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Identity ID of the admin who performed the action.</summary>
    public Guid AdminUserId { get; set; }

    /// <summary>Action label, e.g. "UserLocked", "TierChanged", "CapabilityDeleted".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Type of the affected entity, e.g. "User", "Capability".</summary>
    public string TargetType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected entity.</summary>
    public Guid TargetId { get; set; }

    /// <summary>Human-readable label for the affected entity (e.g. email, process type).</summary>
    public string TargetLabel { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the action.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────────
    public ApplicationUser? Admin { get; set; }
}
