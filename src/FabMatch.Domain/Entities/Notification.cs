using FabMatch.Domain.Common;
using FabMatch.Domain.Enums;

namespace FabMatch.Domain.Entities;

/// <summary>
/// In-app notification delivered to an <see cref="ApplicationUser"/>.
/// Real-time delivery is handled via SignalR; this entity persists the history.
/// </summary>
public sealed class Notification : BaseEntity
{
    // ── Target ─────────────────────────────────────────────────────

    /// <summary>FK to the target user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Navigation to the target user.</summary>
    public ApplicationUser User { get; private set; } = null!;

    // ── Content ────────────────────────────────────────────────────

    /// <summary>Notification category.</summary>
    public NotificationType Type { get; private set; }

    /// <summary>Short notification title (shown in the notification badge).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Full notification message body.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Optional deep-link URL (relative) to navigate the user when clicked.
    /// E.g. "/projects/123/matches".
    /// </summary>
    public string? ActionUrl { get; private set; }

    // ── Read state ─────────────────────────────────────────────────

    /// <summary>Whether the user has read this notification.</summary>
    public bool IsRead { get; private set; }

    /// <summary>UTC timestamp when the notification was read. Null if unread.</summary>
    public DateTime? ReadAt { get; private set; }

    // ── Factory / actions ──────────────────────────────────────────

    /// <summary>Creates a new unread notification.</summary>
    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            IsRead = false
        };
    }

    /// <summary>Marks the notification as read.</summary>
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            Touch();
        }
    }
}
