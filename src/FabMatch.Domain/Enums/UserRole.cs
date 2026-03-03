namespace FabMatch.Domain.Enums;

/// <summary>
/// Roles that a platform user can hold.
/// A user may be both Client and Supplier simultaneously.
/// </summary>
public enum UserRole
{
    /// <summary>Standard buyer: creates projects and searches for suppliers.</summary>
    Client = 1,

    /// <summary>Manufacturer: presents capabilities and receives match requests.</summary>
    Supplier = 2,

    /// <summary>Platform administrator.</summary>
    Admin = 3
}
