using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for <see cref="Supplier"/> profiles with domain-specific queries.
/// </summary>
public interface ISupplierRepository : IRepository<Supplier>
{
    /// <summary>Retrieves the supplier profile linked to the given user.</summary>
    Task<Supplier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Full-text and capability-based search across company name, materials and processes.
    /// </summary>
    Task<IReadOnlyList<Supplier>> SearchAsync(
        string? searchTerm,
        IEnumerable<string>? processes = null,
        IEnumerable<string>? materials = null,
        string? country = null,
        int skip = 0,
        int take = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the supplier with all production capabilities eagerly loaded.
    /// </summary>
    Task<Supplier?> GetWithCapabilitiesAsync(
        Guid supplierId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all suppliers that have a stored embedding vector.
    /// Used by the AI matching engine to compute cosine similarity.
    /// </summary>
    Task<IReadOnlyList<Supplier>> GetAllWithEmbeddingsAsync(CancellationToken ct = default);
}
