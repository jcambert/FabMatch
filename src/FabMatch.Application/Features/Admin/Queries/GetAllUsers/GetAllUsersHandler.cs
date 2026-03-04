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

        var users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(ct);

        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new AdminUserDto(
                user.Id,
                user.Email ?? "",
                user.FullName,
                roles.Contains("Client"),
                roles.Contains("Supplier"),
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.Tier,
                user.CreatedAt,
                user.LastLoginAt));
        }
        return result;
    }
}
