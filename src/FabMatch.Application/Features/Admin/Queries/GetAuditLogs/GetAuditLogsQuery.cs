using FabMatch.Application.Common.Models;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetAuditLogs;

/// <summary>Returns the admin audit log, optionally filtered by action type.</summary>
public sealed record GetAuditLogsQuery(string? ActionFilter = null)
    : IQuery<IReadOnlyList<AuditLogDto>>;
