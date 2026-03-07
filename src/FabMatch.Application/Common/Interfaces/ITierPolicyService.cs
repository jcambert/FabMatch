using FabMatch.Domain.Enums;

namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Enforces subscription-tier limits for feature access.
/// Inject into application command handlers to gate paid features.
/// </summary>
public interface ITierPolicyService
{
    /// <summary>Checks whether the client can create another project given their tier.</summary>
    Task<(bool Allowed, string? Reason)> CanCreateProjectAsync(Guid clientId, CancellationToken ct = default);

    /// <summary>Checks whether the client can trigger an AI analysis given their tier.</summary>
    Task<(bool Allowed, string? Reason)> CanRunAnalysisAsync(Guid clientId, CancellationToken ct = default);

    /// <summary>Returns the feature limits for the given subscription tier.</summary>
    TierLimits GetLimits(SubscriptionTier tier);
}

/// <summary>Describes the feature limits for a subscription tier.</summary>
public sealed record TierLimits(
    int MaxProjects,         // -1 = unlimited
    int MaxAnalysesPerMonth, // -1 = unlimited
    bool UnlimitedMatching,
    bool PrioritySupport,
    string[] Features);
