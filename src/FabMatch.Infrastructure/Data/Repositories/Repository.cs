using System.Linq.Expressions;
using FabMatch.Domain.Common;
using FabMatch.Domain.Interfaces.Repositories;
using FabMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>
/// Generic EF Core repository implementation for all <see cref="BaseEntity"/> entities.
/// All queries automatically exclude soft-deleted records via EF Core global query filters.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Db;
    protected readonly DbSet<TEntity> Set;

    public Repository(ApplicationDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Set.FindAsync([id], ct);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await Set.ToListAsync(ct);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await Set.Where(predicate).ToListAsync(ct);

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(predicate, ct);

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await Set.AnyAsync(predicate, ct);

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    /// <inheritdoc />
    public virtual void Update(TEntity entity) => Set.Update(entity);

    /// <inheritdoc />
    public virtual void Remove(TEntity entity) => Set.Remove(entity);

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default)
        => predicate is null
            ? await Set.CountAsync(ct)
            : await Set.CountAsync(predicate, ct);

    /// <inheritdoc />
    public virtual IQueryable<TEntity> Query() => Set.AsQueryable();
}
