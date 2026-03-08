namespace FabMatch.Application.Common.Models;

/// <summary>Projection of an AdminAuditLog for display in the admin panel.</summary>
public sealed record AuditLogDto(
    Guid Id,
    Guid AdminUserId,
    string AdminName,
    string Action,
    string TargetType,
    Guid TargetId,
    string TargetLabel,
    DateTime CreatedAt);
