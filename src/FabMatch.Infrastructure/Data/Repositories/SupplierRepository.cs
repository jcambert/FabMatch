using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="ISupplierRepository"/>.</summary>
public sealed class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<Supplier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Supplier>> SearchAsync(
        string? searchTerm,
        IEnumerable<string>? processes = null,
        IEnumerable<string>? materials = null,
        string? country = null,
        int skip = 0,
        int take = 20,
        CancellationToken ct = default)
    {
        var query = Set.Include(s => s.ProductionCapabilities).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // EF.Functions.ILike leverages the pg_trgm GIN index for fast case-insensitive search.
            var pattern = $"%{searchTerm}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.CompanyName, pattern) ||
                EF.Functions.ILike(s.Presentation, pattern));
        }

        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(s => s.Country == country);

        if (processes?.Any() == true)
        {
            var processList = processes.ToList();
            query = query.Where(s =>
                s.ProductionCapabilities.Any(c => processList.Contains(c.ProcessType)));
        }

        return await query.OrderBy(s => s.CompanyName).Skip(skip).Take(take).ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Supplier?> GetWithCapabilitiesAsync(Guid supplierId, CancellationToken ct = default)
        => await Set
            .Include(s => s.ProductionCapabilities)
            .FirstOrDefaultAsync(s => s.Id == supplierId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Supplier>> GetAllWithEmbeddingsAsync(CancellationToken ct = default)
        => await Set
            .Include(s => s.ProductionCapabilities)
            .Where(s => s.EmbeddingVector != null)
            .ToListAsync(ct);
}
