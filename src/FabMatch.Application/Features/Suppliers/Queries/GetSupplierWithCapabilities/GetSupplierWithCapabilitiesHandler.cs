using FabMatch.Domain.Interfaces;
using Mediator;

namespace FabMatch.Application.Features.Suppliers.Queries.GetSupplierWithCapabilities;

/// <summary>Returns the full supplier profile with capabilities.</summary>
public sealed class GetSupplierWithCapabilitiesHandler
    : IQueryHandler<GetSupplierWithCapabilitiesQuery, SupplierDetailDto?>
{
    private readonly IUnitOfWork _uow;
    public GetSupplierWithCapabilitiesHandler(IUnitOfWork uow) => _uow = uow;

    public async ValueTask<SupplierDetailDto?> Handle(
        GetSupplierWithCapabilitiesQuery query, CancellationToken ct)
    {
        var s = await _uow.Suppliers.GetWithCapabilitiesAsync(query.SupplierId, ct);
        if (s is null) return null;

        return new SupplierDetailDto(
            s.Id, s.UserId, s.CompanyName, s.Presentation, s.Country,
            s.WebsiteUrl, s.FoundedYear, s.CompanySize,
            s.Certifications, s.Materials, s.AnnualCapacityTonnes,
            s.EmbeddingVector is { Length: > 0 },
            s.ProductionCapabilities
                .Where(c => !c.IsDeleted)
                .Select(c => new CapabilityDto(
                    c.Id, c.ProcessType, c.EquipmentName,
                    c.MaxThicknessMm, c.MaxDimensionXMm, c.MaxDimensionYMm,
                    c.ToleranceMm, c.Notes))
                .ToList());
    }
}
