using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for <see cref="Notification"/> entities with user-specific queries.
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>Returns all notifications for a user, newest first.</summary>
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns unread notifications for a user.</summary>
    Task<IReadOnlyList<Notification>> GetUnreadAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the count of unread notifications for a user.</summary>
    Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Marks all unread notifications for a user as read.</summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
