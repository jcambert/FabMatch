using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Admin.Queries.GetAllSuppliers;

/// <summary>Admin query: returns all supplier profiles with their associated user info.</summary>
public sealed record GetAllSuppliersAdminQuery(string? Search = null) : IQuery<IReadOnlyList<AdminSupplierDto>>;

public sealed record AdminSupplierDto(
    Guid SupplierId,
    Guid UserId,
    string Email,
    string FullName,
    string CompanyName,
    string Country,
    string? CompanySize,
    int CapabilityCount,
    bool HasEmbedding,
    SubscriptionTier Tier,
    bool LockoutEnabled,
    DateTime CreatedAt);
