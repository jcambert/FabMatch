using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// SignalR-backed implementation of <see cref="INotificationHubService"/>.
/// Sends real-time notifications to connected browser clients.
/// </summary>
public sealed class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<FabMatchNotificationHub> _hubContext;
    private readonly ILogger<NotificationHubService> _logger;

    public NotificationHubService(
        IHubContext<FabMatchNotificationHub> hubContext,
        ILogger<NotificationHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendToUserAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Sending {Type} notification to user {UserId}", type, userId);
        await _hubContext.Clients
            .User(userId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                type = type.ToString(),
                title,
                message,
                actionUrl
            }, ct);
    }

    /// <inheritdoc />
    public async Task BroadcastAsync(
        NotificationType type,
        string title,
        string message,
        CancellationToken ct = default)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            type = type.ToString(),
            title,
            message
        }, ct);
    }
}

/// <summary>
/// SignalR hub used by <see cref="NotificationHubService"/> to push notifications.
/// Clients connect to <c>/hubs/notifications</c>.
/// </summary>
public sealed class FabMatchNotificationHub : Hub
{
    /// <summary>
    /// Called when a client requests its unread notification count.
    /// This is a placeholder – the count is pushed by the server, not pulled.
    /// </summary>
    public async Task RequestUnreadCount()
        => await Clients.Caller.SendAsync("UnreadCount", 0);
}
