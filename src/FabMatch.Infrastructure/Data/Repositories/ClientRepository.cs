using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IClientRepository"/>.</summary>
public sealed class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Client>> SearchAsync(
        string searchTerm,
        int skip = 0,
        int take = 20,
        CancellationToken ct = default)
    {
        var term = searchTerm.ToLower();
        return await Set
            .Where(c => c.CompanyName.ToLower().Contains(term)
                     || c.Industry.ToLower().Contains(term)
                     || c.Description.ToLower().Contains(term))
            .OrderBy(c => c.CompanyName)
            .Skip(skip).Take(take)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Client?> GetWithProjectsAsync(Guid clientId, CancellationToken ct = default)
        => await Set
            .Include(c => c.Projects)
                .ThenInclude(p => p.Plans)
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);
}
