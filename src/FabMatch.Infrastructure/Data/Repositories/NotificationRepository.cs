using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="INotificationRepository"/>.</summary>
public sealed class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Set
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetUnreadAsync(Guid userId, CancellationToken ct = default)
        => await Set
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
        => await Set.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        if (Db.Database.IsRelational())
        {
            await Set
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
            return;
        }

        var notifications = await Set
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        if (notifications.Count > 0)
        {
            await Db.SaveChangesAsync(ct);
        }
    }
}
