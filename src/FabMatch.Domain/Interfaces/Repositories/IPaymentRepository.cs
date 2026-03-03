using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>Repository for <see cref="Payment"/> entities.</summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>Returns all payments for a user, newest first.</summary>
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
