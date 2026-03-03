using FabMatch.Domain.Interfaces;
using Mediator;

namespace FabMatch.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>Returns notifications for a user, optionally filtered to unread only.</summary>
public sealed class GetNotificationsHandler
    : IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly IUnitOfWork _uow;

    public GetNotificationsHandler(IUnitOfWork uow) => _uow = uow;

    public async ValueTask<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery query, CancellationToken ct)
    {
        var notifications = query.UnreadOnly
            ? await _uow.Notifications.GetUnreadAsync(query.UserId, ct)
            : await _uow.Notifications.GetByUserIdAsync(query.UserId, ct);

        return notifications.Select(n => new NotificationDto(
            n.Id, n.Type, n.Title, n.Message, n.ActionUrl,
            n.IsRead, n.ReadAt, n.CreatedAt)).ToList();
    }
}
