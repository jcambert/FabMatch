using FabMatch.Application.Common.Models;

namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Records admin actions in a persistent audit trail and provides read access to the log.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Persists an audit entry for the currently authenticated admin.
    /// The admin identity is resolved from the ambient HTTP context.
    /// </summary>
    Task LogAsync(
        string action,
        string targetType,
        Guid targetId,
        string targetLabel,
        CancellationToken ct = default);

    /// <summary>Returns the audit log, newest entries first, optionally filtered by action.</summary>
    Task<IReadOnlyList<AuditLogDto>> GetLogsAsync(
        string? actionFilter = null,
        CancellationToken ct = default);
}
