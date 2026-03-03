namespace FabMatch.Domain.Enums;

/// <summary>Lifecycle states of an AI-generated match between a project and a supplier.</summary>
public enum MatchStatus
{
    /// <summary>Proposed by AI – awaiting client review.</summary>
    Proposed = 0,

    /// <summary>Accepted by the client – supplier notified.</summary>
    AcceptedByClient = 1,

    /// <summary>Accepted by the supplier – negotiations may begin.</summary>
    AcceptedBySupplier = 2,

    /// <summary>Rejected by the client.</summary>
    RejectedByClient = 3,

    /// <summary>Rejected by the supplier.</summary>
    RejectedBySupplier = 4,

    /// <summary>Finalised – contract awarded.</summary>
    Finalised = 5
}
