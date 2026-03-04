using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>Repository for <see cref="ProductionCapability"/> entities.</summary>
public interface ICapabilityRepository : IRepository<ProductionCapability>
{
    /// <summary>Returns all active capabilities for a supplier.</summary>
    Task<IReadOnlyList<ProductionCapability>> GetBySupplierIdAsync(
        Guid supplierId, CancellationToken ct = default);
}
