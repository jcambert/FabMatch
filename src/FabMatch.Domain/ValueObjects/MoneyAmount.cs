namespace FabMatch.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// </summary>
/// <param name="Amount">The numeric amount (non-negative).</param>
/// <param name="Currency">ISO 4217 currency code (e.g. "EUR", "USD").</param>
public sealed record MoneyAmount(decimal Amount, string Currency = "EUR")
{
    /// <summary>Creates a zero-value amount in the given currency.</summary>
    public static MoneyAmount Zero(string currency = "EUR") => new(0m, currency);

    /// <summary>Returns a formatted string like "1 234,56 EUR".</summary>
    public override string ToString() =>
        $"{Amount:N2} {Currency}";
}
