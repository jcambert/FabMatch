using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Infrastructure.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IProjectRepository"/>.</summary>
public sealed class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext db) : base(db) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Project>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
        => await Set
            .Include(p => p.Plans)
            .Include(p => p.Matches)
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Project>> GetPublicActiveProjectsAsync(
        int skip = 0, int take = 20, CancellationToken ct = default)
        => await Set
            .Where(p => p.IsPublic && p.Status == ProjectStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<Project?> GetWithPlansAndMatchesAsync(Guid projectId, CancellationToken ct = default)
        => await Set
            .Include(p => p.Client)
            .Include(p => p.Plans)
                .ThenInclude(pl => pl.LatestAnalysis)
            .Include(p => p.Matches)
                .ThenInclude(m => m.Supplier)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Project>> GetProjectsForMatchingAsync(
        ProjectStatus status = ProjectStatus.Active,
        CancellationToken ct = default)
        => await Set
            .Include(p => p.Client)
            .Where(p => p.Status == status && p.EmbeddingVector != null)
            .ToListAsync(ct);
}
