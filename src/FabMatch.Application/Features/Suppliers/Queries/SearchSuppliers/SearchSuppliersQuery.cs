using Mediator;

namespace FabMatch.Application.Features.Suppliers.Queries.SearchSuppliers;

/// <summary>Supplier search query with optional process/material/country filters.</summary>
public sealed record SearchSuppliersQuery(
    string? SearchTerm,
    IEnumerable<string>? Processes = null,
    IEnumerable<string>? Materials = null,
    string? Country = null,
    int Skip = 0,
    int Take = 20) : IQuery<IReadOnlyList<SupplierSearchResultDto>>;

/// <summary>Lightweight supplier result for search lists.</summary>
public sealed record SupplierSearchResultDto(
    Guid Id,
    string CompanyName,
    string Presentation,
    string Country,
    IReadOnlyList<string> Certifications,
    IReadOnlyList<string> Materials,
    int CapabilityCount,
    bool HasEmbedding);
