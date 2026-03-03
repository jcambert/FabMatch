namespace FabMatch.Domain.ValueObjects;

/// <summary>
/// Value object representing a postal address.
/// Equality is structural (all fields must match).
/// </summary>
public sealed record Address(
    string Street,
    string City,
    string PostalCode,
    string Country)
{
    /// <summary>Returns the address as a single-line string.</summary>
    public override string ToString() =>
        $"{Street}, {PostalCode} {City}, {Country}";
}
