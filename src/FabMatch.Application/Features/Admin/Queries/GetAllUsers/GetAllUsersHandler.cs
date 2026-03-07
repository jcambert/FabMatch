using FabMatch.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FabMatch.Application.Features.Admin.Queries.GetAllUsers;

/// <summary>Returns all users with their roles, filtered by optional search term.</summary>
public sealed class GetAllUsersHandler
    : IQueryHandler<GetAllUsersQuery, IReadOnlyList<AdminUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async ValueTask<IReadOnlyList<AdminUserDto>> Handle(
        GetAllUsersQuery query, CancellationToken ct)
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        // Materialize the page first — DbContext reader fully closed after this.
        var users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(ct);

        if (users.Count == 0) return [];

        // Fetch all role sets in three sequential queries (no concurrent DbContext access).
        // Avoids the N+1 GetRolesAsync-per-user pattern that triggers the
        // "second operation started" concurrency exception in Blazor Server.
        var clientIds   = (await _userManager.GetUsersInRoleAsync("Client"))
                            .Select(u => u.Id).ToHashSet();
        var supplierIds = (await _userManager.GetUsersInRoleAsync("Supplier"))
                            .Select(u => u.Id).ToHashSet();

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.Email ?? "",
            u.FullName,
            clientIds.Contains(u.Id),
            supplierIds.Contains(u.Id),
            u.EmailConfirmed,
            u.LockoutEnabled,
            u.Tier,
            u.CreatedAt,
            u.LastLoginAt)).ToList();
    }
}
