using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="ICapabilityRepository"/>.</summary>
public sealed class CapabilityRepository : Repository<ProductionCapability>, ICapabilityRepository
{
    public CapabilityRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductionCapability>> GetBySupplierIdAsync(
        Guid supplierId, CancellationToken ct = default)
        => await Set
            .Where(c => c.SupplierId == supplierId)
            .OrderBy(c => c.ProcessType)
            .ToListAsync(ct);
}
