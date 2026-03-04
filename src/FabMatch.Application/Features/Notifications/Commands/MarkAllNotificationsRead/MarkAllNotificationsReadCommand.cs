using Mediator;

namespace FabMatch.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

/// <summary>Marks all unread notifications as read for the given user.</summary>
public sealed record MarkAllNotificationsReadCommand(Guid UserId) : ICommand<bool>;
