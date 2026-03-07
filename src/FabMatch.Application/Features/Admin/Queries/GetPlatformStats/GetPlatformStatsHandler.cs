using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Application.Features.Admin.Queries.GetPlatformStats;

public sealed class GetPlatformStatsHandler : IQueryHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetPlatformStatsHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async ValueTask<PlatformStatsDto> Handle(GetPlatformStatsQuery query, CancellationToken ct)
    {
        // All counts are sequential — a single scoped DbContext cannot handle concurrent async ops.
        var totalUsers        = await _userManager.Users.CountAsync(ct);
        var freeUsers         = await _userManager.Users.CountAsync(u => u.Tier == SubscriptionTier.Free, ct);
        var starterUsers      = await _userManager.Users.CountAsync(u => u.Tier == SubscriptionTier.Starter, ct);
        var professionalUsers = await _userManager.Users.CountAsync(u => u.Tier == SubscriptionTier.Professional, ct);
        var enterpriseUsers   = await _userManager.Users.CountAsync(u => u.Tier == SubscriptionTier.Enterprise, ct);

        var totalClients   = await _uow.Clients.CountAsync(ct: ct);
        var totalSuppliers = await _uow.Suppliers.CountAsync(ct: ct);
        var totalProjects  = await _uow.Projects.CountAsync(ct: ct);
        var activeProjects = await _uow.Projects.CountAsync(
            p => p.Status == ProjectStatus.Active, ct);
        var totalMatches   = await _uow.Matches.CountAsync(ct: ct);
        var finalisedMatches = await _uow.Matches.CountAsync(
            m => m.Status == MatchStatus.Finalised, ct);
        var totalPlans    = await _uow.Plans.CountAsync(ct: ct);
        var totalAnalyses = await _uow.Analyses.CountAsync(ct: ct);

        // Pricing: Starter €49/mo, Professional €149/mo, Enterprise €499/mo
        var estimatedMrr = (starterUsers * 49m) + (professionalUsers * 149m) + (enterpriseUsers * 499m);

        return new PlatformStatsDto(
            totalUsers, totalClients, totalSuppliers,
            activeProjects, totalProjects,
            totalMatches, finalisedMatches,
            totalPlans, totalAnalyses,
            freeUsers, starterUsers, professionalUsers, enterpriseUsers,
            estimatedMrr);
    }
}
