using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>Query to retrieve notifications for a user.</summary>
public sealed record GetNotificationsQuery(Guid UserId, bool UnreadOnly = false)
    : IQuery<IReadOnlyList<NotificationDto>>;

/// <summary>DTO for a single notification.</summary>
public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    string? ActionUrl,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);
