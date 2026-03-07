using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetPlatformStats;

/// <summary>Returns aggregated platform statistics for the admin dashboard.</summary>
public sealed record GetPlatformStatsQuery : IQuery<PlatformStatsDto>;

public sealed record PlatformStatsDto(
    int TotalUsers,
    int TotalClients,
    int TotalSuppliers,
    int ActiveProjects,
    int TotalProjects,
    int TotalMatches,
    int FinalisedMatches,
    int TotalPlans,
    int TotalAnalyses,
    int FreeUsers,
    int StarterUsers,
    int ProfessionalUsers,
    int EnterpriseUsers,
    decimal EstimatedMRR);
