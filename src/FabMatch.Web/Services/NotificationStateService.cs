namespace FabMatch.Web.Services;

/// <summary>
/// Scoped service that tracks the unread notification count for the current user.
/// The count is updated by the SignalR notification hub and consumed by the layout.
/// </summary>
public sealed class NotificationStateService
{
    private int _unreadCount;

    /// <summary>Current number of unread notifications.</summary>
    public int UnreadCount => _unreadCount;

    /// <summary>Raised whenever the unread count changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the unread count and raises the change event.</summary>
    public void SetUnreadCount(int count)
    {
        _unreadCount = count;
        OnChange?.Invoke();
    }

    /// <summary>Decrements the unread count by the specified amount.</summary>
    public void Decrement(int by = 1)
    {
        _unreadCount = Math.Max(0, _unreadCount - by);
        OnChange?.Invoke();
    }
}
