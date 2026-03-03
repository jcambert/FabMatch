using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>Repository for <see cref="Plan"/> entities.</summary>
public interface IPlanRepository : IRepository<Plan>
{
    /// <summary>Returns all plans belonging to a project.</summary>
    Task<IReadOnlyList<Plan>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Returns the plan with its latest analysis eagerly loaded.</summary>
    Task<Plan?> GetWithAnalysisAsync(Guid planId, CancellationToken ct = default);
}
