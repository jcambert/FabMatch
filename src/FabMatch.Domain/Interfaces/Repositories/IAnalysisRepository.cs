using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>Repository for <see cref="Analysis"/> entities.</summary>
public interface IAnalysisRepository : IRepository<Analysis>
{
    /// <summary>Returns all analyses for a specific plan, ordered newest first.</summary>
    Task<IReadOnlyList<Analysis>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);

    /// <summary>
    /// Returns the number of AI analyses run for a client during the current calendar month.
    /// Used to enforce tier limits.
    /// </summary>
    Task<int> CountByClientThisMonthAsync(Guid clientId, CancellationToken ct = default);
}
