namespace FabMatch.Domain.Enums;

/// <summary>Platform subscription tiers available to users.</summary>
public enum SubscriptionTier
{
    /// <summary>Free tier with limited features.</summary>
    Free = 0,

    /// <summary>Starter plan – basic matching features.</summary>
    Starter = 1,

    /// <summary>Professional plan – advanced AI analysis and unlimited projects.</summary>
    Professional = 2,

    /// <summary>Enterprise plan – custom SLA, dedicated support.</summary>
    Enterprise = 3
}
