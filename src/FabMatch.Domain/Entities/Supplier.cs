using FabMatch.Domain.Common;
using FabMatch.Domain.ValueObjects;

namespace FabMatch.Domain.Entities;

/// <summary>
/// Supplier (manufacturer) profile attached to an <see cref="ApplicationUser"/>.
/// Describes the company's manufacturing capabilities used for AI-based matching.
/// </summary>
public sealed class Supplier : BaseEntity
{
    // ── Identity ───────────────────────────────────────────────────

    /// <summary>FK to the owning <see cref="ApplicationUser"/>.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Navigation to the owning user.</summary>
    public ApplicationUser User { get; private set; } = null!;

    // ── Company information ────────────────────────────────────────

    /// <summary>Legal company name.</summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>Company presentation / description shown to clients.</summary>
    public string Presentation { get; private set; } = string.Empty;

    /// <summary>Year the company was founded.</summary>
    public int? FoundedYear { get; private set; }

    /// <summary>Number of employees.</summary>
    public string? CompanySize { get; private set; }

    /// <summary>ISO 3166-1 alpha-2 country code where manufacturing takes place.</summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>Website URL.</summary>
    public string? WebsiteUrl { get; private set; }

    /// <summary>List of certifications held (e.g. "ISO 9001", "EN 15085").</summary>
    public List<string> Certifications { get; private set; } = [];

    /// <summary>Primary materials processed (e.g. "Steel", "Aluminium", "Stainless steel").</summary>
    public List<string> Materials { get; private set; } = [];

    /// <summary>
    /// Typical annual production capacity in tonnes.
    /// Used as a coarse matching signal.
    /// </summary>
    public decimal? AnnualCapacityTonnes { get; private set; }

    /// <summary>Minimum order value accepted.</summary>
    public MoneyAmount? MinOrderValue { get; private set; }

    /// <summary>Maximum order value accepted.</summary>
    public MoneyAmount? MaxOrderValue { get; private set; }

    /// <summary>
    /// AI-generated embedding vector for semantic similarity matching.
    /// Stored as a JSON-serialised float array.
    /// </summary>
    public float[]? EmbeddingVector { get; private set; }

    // ── Navigation properties ──────────────────────────────────────

    /// <summary>Production capabilities (machines, processes, tolerances).</summary>
    public ICollection<ProductionCapability> ProductionCapabilities { get; private set; } = [];

    /// <summary>Matches where this supplier is a candidate.</summary>
    public ICollection<Match> Matches { get; private set; } = [];

    // ── Factory / update methods ───────────────────────────────────

    /// <summary>Creates a new supplier profile.</summary>
    public static Supplier Create(
        Guid userId,
        string companyName,
        string presentation,
        string country,
        string? websiteUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        return new Supplier
        {
            UserId = userId,
            CompanyName = companyName,
            Presentation = presentation,
            Country = country,
            WebsiteUrl = websiteUrl
        };
    }

    /// <summary>Updates the supplier profile fields.</summary>
    public void Update(
        string companyName,
        string presentation,
        string country,
        string? websiteUrl,
        int? foundedYear,
        string? companySize,
        List<string>? certifications,
        List<string>? materials,
        decimal? annualCapacityTonnes,
        MoneyAmount? minOrderValue,
        MoneyAmount? maxOrderValue)
    {
        CompanyName = companyName;
        Presentation = presentation;
        Country = country;
        WebsiteUrl = websiteUrl;
        FoundedYear = foundedYear;
        CompanySize = companySize;
        Certifications = certifications ?? [];
        Materials = materials ?? [];
        AnnualCapacityTonnes = annualCapacityTonnes;
        MinOrderValue = minOrderValue;
        MaxOrderValue = maxOrderValue;
        Touch();
    }

    /// <summary>Stores the AI embedding vector computed for this supplier.</summary>
    public void SetEmbedding(float[] vector)
    {
        EmbeddingVector = vector;
        Touch();
    }
}
