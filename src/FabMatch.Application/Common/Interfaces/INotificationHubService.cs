using FabMatch.Domain.Enums;

namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Sends real-time push notifications to connected clients via SignalR.
/// </summary>
public interface INotificationHubService
{
    /// <summary>
    /// Sends a notification to a specific user (all of their connections).
    /// </summary>
    Task SendToUserAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        CancellationToken ct = default);

    /// <summary>
    /// Broadcasts a notification to all connected users (admin announcements).
    /// </summary>
    Task BroadcastAsync(
        NotificationType type,
        string title,
        string message,
        CancellationToken ct = default);
}
