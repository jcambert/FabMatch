using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using FabMatch.Domain.Entities;
using FabMatch.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// Persists admin audit entries and reads the audit log from PostgreSQL.
/// Resolves the acting admin from the ambient HTTP context.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogger(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        string targetType,
        Guid targetId,
        string targetLabel,
        CancellationToken ct = default)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User
            ?.FindFirstValue(ClaimTypes.NameIdentifier);

        Guid.TryParse(userIdStr, out var adminId);

        _db.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminId,
            Action      = action,
            TargetType  = targetType,
            TargetId    = targetId,
            TargetLabel = targetLabel,
            CreatedAt   = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetLogsAsync(
        string? actionFilter = null,
        CancellationToken ct = default)
    {
        var query = _db.AdminAuditLogs
            .AsNoTracking()
            .Include(l => l.Admin)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(actionFilter))
            query = query.Where(l => l.Action == actionFilter);

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(500)
            .ToListAsync(ct);

        return logs.Select(l => new AuditLogDto(
            l.Id,
            l.AdminUserId,
            l.Admin?.FullName.Trim() is { Length: > 0 } name ? name : l.Admin?.Email ?? "Admin",
            l.Action,
            l.TargetType,
            l.TargetId,
            l.TargetLabel,
            l.CreatedAt)).ToList();
    }
}
