using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetAllClients;

/// <summary>Admin query: returns all client profiles with their associated user info.</summary>
public sealed record GetAllClientsAdminQuery(string? Search = null) : IQuery<IReadOnlyList<AdminClientDto>>;

public sealed record AdminClientDto(
    Guid ClientId,
    Guid UserId,
    string Email,
    string FullName,
    string CompanyName,
    string Industry,
    string? WebsiteUrl,
    string? CompanySize,
    SubscriptionTier Tier,
    bool LockoutEnabled,
    DateTime CreatedAt);
