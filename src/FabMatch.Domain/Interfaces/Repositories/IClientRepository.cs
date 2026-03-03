using FabMatch.Domain.Entities;

namespace FabMatch.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for <see cref="Client"/> profiles with domain-specific queries.
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    /// <summary>Retrieves the client profile linked to the given user.</summary>
    Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Full-text search across company name, industry and description.
    /// </summary>
    Task<IReadOnlyList<Client>> SearchAsync(
        string searchTerm,
        int skip = 0,
        int take = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the client profile with all projects and their plans eagerly loaded.
    /// </summary>
    Task<Client?> GetWithProjectsAsync(Guid clientId, CancellationToken ct = default);
}
