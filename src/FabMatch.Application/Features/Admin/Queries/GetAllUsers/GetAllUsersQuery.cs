using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetAllUsers;

/// <summary>Admin query to retrieve all platform users with their profile status.</summary>
public sealed record GetAllUsersQuery(string? SearchTerm = null, int Skip = 0, int Take = 50)
    : IQuery<IReadOnlyList<AdminUserDto>>;

/// <summary>Summary DTO used in the admin user list.</summary>
public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsClient,
    bool IsSupplier,
    bool EmailConfirmed,
    bool LockoutEnabled,
    SubscriptionTier Tier,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
