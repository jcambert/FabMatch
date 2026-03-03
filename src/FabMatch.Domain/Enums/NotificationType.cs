namespace FabMatch.Domain.Enums;

/// <summary>Categories of platform notifications.</summary>
public enum NotificationType
{
    /// <summary>A new AI-proposed match is available.</summary>
    NewMatch = 1,

    /// <summary>A match has been accepted.</summary>
    MatchAccepted = 2,

    /// <summary>A match has been rejected.</summary>
    MatchRejected = 3,

    /// <summary>A plan analysis has completed.</summary>
    AnalysisComplete = 4,

    /// <summary>A payment has been processed.</summary>
    PaymentProcessed = 5,

    /// <summary>A new project has been published (visible to suppliers).</summary>
    NewProject = 6,

    /// <summary>General informational message.</summary>
    Info = 7
}
