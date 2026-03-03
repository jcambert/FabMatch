using FabMatch.Domain.Interfaces.Repositories;

namespace FabMatch.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern – groups all repository access within a single database transaction.
/// Call <see cref="SaveChangesAsync"/> to atomically persist all pending changes.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Repository for client profiles.</summary>
    IClientRepository Clients { get; }

    /// <summary>Repository for supplier profiles.</summary>
    ISupplierRepository Suppliers { get; }

    /// <summary>Repository for projects.</summary>
    IProjectRepository Projects { get; }

    /// <summary>Repository for technical plans.</summary>
    IPlanRepository Plans { get; }

    /// <summary>Repository for AI analysis results.</summary>
    IAnalysisRepository Analyses { get; }

    /// <summary>Repository for matches.</summary>
    IMatchRepository Matches { get; }

    /// <summary>Repository for notifications.</summary>
    INotificationRepository Notifications { get; }

    /// <summary>
    /// Persists all pending changes in the current transaction.
    /// </summary>
    /// <returns>Number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Begins an explicit database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Commits the current transaction.</summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>Rolls back the current transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
