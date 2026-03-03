using System.Linq.Expressions;
using FabMatch.Domain.Common;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>
/// Generic repository interface providing basic CRUD and query operations
/// for all domain entities that extend <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this repository.</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>Retrieves an entity by its primary key.</summary>
    /// <returns>The entity, or <c>null</c> if not found.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all non-deleted entities.</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns entities matching the given predicate.</summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Returns the first entity matching the predicate, or null.</summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Returns whether any entity matches the predicate.</summary>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>Adds a new entity to the context (not yet persisted).</summary>
    Task AddAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Updates an existing entity in the context.</summary>
    void Update(TEntity entity);

    /// <summary>Removes an entity from the context (hard delete).</summary>
    void Remove(TEntity entity);

    /// <summary>Returns the total count of non-deleted entities.</summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a queryable for complex queries that need EF Core operators.
    /// Use with care; prefer explicit methods for common queries.
    /// </summary>
    IQueryable<TEntity> Query();
}
