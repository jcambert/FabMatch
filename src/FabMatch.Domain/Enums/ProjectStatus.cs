namespace FabMatch.Domain.Enums;

/// <summary>Lifecycle states of a client project.</summary>
public enum ProjectStatus
{
    /// <summary>Draft – not yet published to suppliers.</summary>
    Draft = 0,

    /// <summary>Active – published and open for matching.</summary>
    Active = 1,

    /// <summary>In negotiation – a supplier has been selected.</summary>
    InNegotiation = 2,

    /// <summary>Awarded – contract signed with a supplier.</summary>
    Awarded = 3,

    /// <summary>Completed – manufacturing finished and delivered.</summary>
    Completed = 4,

    /// <summary>Cancelled – project cancelled by the client.</summary>
    Cancelled = 5
}
