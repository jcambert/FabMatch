using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetAuditLogs;

public sealed class GetAuditLogsHandler
    : IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IAuditLogger _auditLogger;

    public GetAuditLogsHandler(IAuditLogger auditLogger)
        => _auditLogger = auditLogger;

    public async ValueTask<IReadOnlyList<AuditLogDto>> Handle(
        GetAuditLogsQuery query, CancellationToken ct)
        => await _auditLogger.GetLogsAsync(query.ActionFilter, ct);
}
