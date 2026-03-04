using FabMatch.Domain.Interfaces;
using Mediator;

namespace FabMatch.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

/// <summary>Marks all unread notifications for a user as read.</summary>
public sealed class MarkAllNotificationsReadHandler : ICommandHandler<MarkAllNotificationsReadCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public MarkAllNotificationsReadHandler(IUnitOfWork uow) => _uow = uow;

    public async ValueTask<bool> Handle(MarkAllNotificationsReadCommand cmd, CancellationToken ct)
    {
        await _uow.Notifications.MarkAllAsReadAsync(cmd.UserId, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
