using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// Enforces subscription-tier limits using the client's project/analysis counts.
/// </summary>
public sealed class TierPolicyService : ITierPolicyService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    // ── Tier definitions ──────────────────────────────────────────────────────
    private static readonly Dictionary<SubscriptionTier, TierLimits> Limits = new()
    {
        [SubscriptionTier.Free] = new TierLimits(
            MaxProjects: 3, MaxAnalysesPerMonth: 5,
            UnlimitedMatching: false, PrioritySupport: false,
            Features: ["3 projets", "5 analyses IA/mois", "Matching de base"]),

        [SubscriptionTier.Starter] = new TierLimits(
            MaxProjects: 15, MaxAnalysesPerMonth: 50,
            UnlimitedMatching: true, PrioritySupport: false,
            Features: ["15 projets", "50 analyses IA/mois", "Matching illimité", "Notifications email"]),

        [SubscriptionTier.Professional] = new TierLimits(
            MaxProjects: -1, MaxAnalysesPerMonth: -1,
            UnlimitedMatching: true, PrioritySupport: false,
            Features: ["Projets illimités", "Analyses illimitées", "Matching illimité", "Export PDF", "Support prioritaire"]),

        [SubscriptionTier.Enterprise] = new TierLimits(
            MaxProjects: -1, MaxAnalysesPerMonth: -1,
            UnlimitedMatching: true, PrioritySupport: true,
            Features: ["Tout Professional", "SLA dédié", "Intégration API", "Compte manager dédié", "White-label"]),
    };

    public TierPolicyService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    /// <inheritdoc />
    public TierLimits GetLimits(SubscriptionTier tier)
        => Limits.TryGetValue(tier, out var l) ? l : Limits[SubscriptionTier.Free];

    /// <inheritdoc />
    public async Task<(bool Allowed, string? Reason)> CanCreateProjectAsync(
        Guid clientId, CancellationToken ct = default)
    {
        var tier = await GetClientTierAsync(clientId, ct);
        var limits = GetLimits(tier);

        if (limits.MaxProjects < 0) return (true, null); // unlimited

        var count = await _uow.Projects.CountAsync(p => p.ClientId == clientId, ct);
        if (count >= limits.MaxProjects)
            return (false,
                $"Votre plan {tier} est limité à {limits.MaxProjects} projet(s). " +
                "Passez à un plan supérieur pour en créer davantage.");

        return (true, null);
    }

    /// <inheritdoc />
    public async Task<(bool Allowed, string? Reason)> CanRunAnalysisAsync(
        Guid clientId, CancellationToken ct = default)
    {
        var tier = await GetClientTierAsync(clientId, ct);
        var limits = GetLimits(tier);

        if (limits.MaxAnalysesPerMonth < 0) return (true, null); // unlimited

        var count = await _uow.Analyses.CountByClientThisMonthAsync(clientId, ct);
        if (count >= limits.MaxAnalysesPerMonth)
            return (false,
                $"Votre plan {tier} est limité à {limits.MaxAnalysesPerMonth} analyse(s) IA par mois. " +
                "Passez à un plan supérieur pour continuer.");

        return (true, null);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task<SubscriptionTier> GetClientTierAsync(Guid clientId, CancellationToken ct)
    {
        var client = await _uow.Clients.GetByIdAsync(clientId, ct);
        if (client is null) return SubscriptionTier.Free;

        var user = await _userManager.FindByIdAsync(client.UserId.ToString());
        return user?.Tier ?? SubscriptionTier.Free;
    }
}
