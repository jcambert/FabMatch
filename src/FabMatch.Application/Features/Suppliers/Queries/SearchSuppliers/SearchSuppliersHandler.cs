using FabMatch.Domain.Interfaces;
using Mediator;

namespace FabMatch.Application.Features.Suppliers.Queries.SearchSuppliers;

/// <summary>Handles supplier search with process/material/country filters.</summary>
public sealed class SearchSuppliersHandler
    : IQueryHandler<SearchSuppliersQuery, IReadOnlyList<SupplierSearchResultDto>>
{
    private readonly IUnitOfWork _uow;
    public SearchSuppliersHandler(IUnitOfWork uow) => _uow = uow;

    public async ValueTask<IReadOnlyList<SupplierSearchResultDto>> Handle(
        SearchSuppliersQuery query, CancellationToken ct)
    {
        var suppliers = await _uow.Suppliers.SearchAsync(
            query.SearchTerm,
            query.Processes,
            query.Materials,
            query.Country,
            query.Skip,
            query.Take,
            ct);

        return suppliers.Select(s => new SupplierSearchResultDto(
            s.Id, s.CompanyName, s.Presentation, s.Country,
            s.Certifications, s.Materials,
            s.ProductionCapabilities.Count(c => !c.IsDeleted),
            s.EmbeddingVector is { Length: > 0 })).ToList();
    }
}
