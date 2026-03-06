using FabMatch.Domain.Interfaces;
using FabMatch.Domain.Interfaces.Repositories;
using FabMatch.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace FabMatch.Infrastructure.Data;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// Lazily creates repository instances so unused repositories don't allocate memory.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private IDbContextTransaction? _transaction;

    // Lazily-initialized repositories
    private IClientRepository? _clients;
    private ISupplierRepository? _suppliers;
    private IProjectRepository? _projects;
    private IPlanRepository? _plans;
    private IAnalysisRepository? _analyses;
    private IMatchRepository? _matches;
    private INotificationRepository? _notifications;
    private ICapabilityRepository? _capabilities;
    private IPaymentRepository? _payments;

    public UnitOfWork(ApplicationDbContext db) => _db = db;

    /// <inheritdoc />
    public IClientRepository Clients => _clients ??= new ClientRepository(_db);

    /// <inheritdoc />
    public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_db);

    /// <inheritdoc />
    public IProjectRepository Projects => _projects ??= new ProjectRepository(_db);

    /// <inheritdoc />
    public IPlanRepository Plans => _plans ??= new PlanRepository(_db);

    /// <inheritdoc />
    public IAnalysisRepository Analyses => _analyses ??= new AnalysisRepository(_db);

    /// <inheritdoc />
    public IMatchRepository Matches => _matches ??= new MatchRepository(_db);

    /// <inheritdoc />
    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_db);

    /// <inheritdoc />
    public ICapabilityRepository Capabilities => _capabilities ??= new CapabilityRepository(_db);

    /// <inheritdoc />
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_db);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();
        await _db.DisposeAsync();
    }
}
