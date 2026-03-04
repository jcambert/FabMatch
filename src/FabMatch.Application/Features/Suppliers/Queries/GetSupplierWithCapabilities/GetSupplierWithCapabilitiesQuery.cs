using Mediator;

namespace FabMatch.Application.Features.Suppliers.Queries.GetSupplierWithCapabilities;

/// <summary>Returns a supplier's full profile including production capabilities.</summary>
public sealed record GetSupplierWithCapabilitiesQuery(Guid SupplierId)
    : IQuery<SupplierDetailDto?>;

/// <summary>Full supplier detail DTO.</summary>
public sealed record SupplierDetailDto(
    Guid Id,
    Guid UserId,
    string CompanyName,
    string Presentation,
    string Country,
    string? WebsiteUrl,
    int? FoundedYear,
    string? CompanySize,
    IReadOnlyList<string> Certifications,
    IReadOnlyList<string> Materials,
    decimal? AnnualCapacityTonnes,
    bool HasEmbedding,
    IReadOnlyList<CapabilityDto> Capabilities);

/// <summary>Production capability DTO.</summary>
public sealed record CapabilityDto(
    Guid Id,
    string ProcessType,
    string? EquipmentName,
    decimal? MaxThicknessMm,
    decimal? MaxDimensionXMm,
    decimal? MaxDimensionYMm,
    decimal? ToleranceMm,
    string? Notes);
