using FabMatch.Domain.Common;

namespace FabMatch.Domain.Entities;

/// <summary>
/// Client (buyer) profile attached to an <see cref="ApplicationUser"/>.
/// A client creates projects and searches for manufacturing suppliers.
/// </summary>
public sealed class Client : BaseEntity
{
    // ── Identity ───────────────────────────────────────────────────

    /// <summary>FK to the owning <see cref="ApplicationUser"/>.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Navigation property to the owning user.</summary>
    public ApplicationUser User { get; private set; } = null!;

    // ── Company information ────────────────────────────────────────

    /// <summary>Legal company name.</summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>Business sector or industry (e.g. "Aerospace", "Automotive").</summary>
    public string Industry { get; private set; } = string.Empty;

    /// <summary>Short public description shown to suppliers.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Website URL of the client company.</summary>
    public string? WebsiteUrl { get; private set; }

    /// <summary>Number of employees (approximate range).</summary>
    public string? CompanySize { get; private set; }

    // ── Navigation properties ──────────────────────────────────────

    /// <summary>Projects created by this client.</summary>
    public ICollection<Project> Projects { get; private set; } = [];

    // ── Factory / update methods ───────────────────────────────────

    /// <summary>
    /// Creates a new client profile.
    /// </summary>
    public static Client Create(
        Guid userId,
        string companyName,
        string industry,
        string description,
        string? websiteUrl = null,
        string? companySize = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        return new Client
        {
            UserId = userId,
            CompanyName = companyName,
            Industry = industry,
            Description = description,
            WebsiteUrl = websiteUrl,
            CompanySize = companySize
        };
    }

    /// <summary>Updates the client's profile information.</summary>
    public void Update(
        string companyName,
        string industry,
        string description,
        string? websiteUrl,
        string? companySize)
    {
        CompanyName = companyName;
        Industry = industry;
        Description = description;
        WebsiteUrl = websiteUrl;
        CompanySize = companySize;
        Touch();
    }
}
