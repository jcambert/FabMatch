using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace FabMatch.Application.Features.Admin.Queries.GetAllSuppliers;

public sealed class GetAllSuppliersAdminHandler
    : IQueryHandler<GetAllSuppliersAdminQuery, IReadOnlyList<AdminSupplierDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllSuppliersAdminHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async ValueTask<IReadOnlyList<AdminSupplierDto>> Handle(
        GetAllSuppliersAdminQuery query, CancellationToken ct)
    {
        var suppliers = await _uow.Suppliers.GetAllAsync(ct);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLower();
            suppliers = suppliers.Where(s =>
                s.CompanyName.ToLower().Contains(term) ||
                s.Country.ToLower().Contains(term) ||
                (s.CompanySize ?? "").ToLower().Contains(term)).ToList();
        }

        var capabilities = await _uow.Capabilities.GetAllAsync(ct);
        var capCountBySupplierId = capabilities
            .GroupBy(c => c.SupplierId)
            .ToDictionary(g => g.Key, g => g.Count());

        var userIds = suppliers.Select(s => s.UserId).ToHashSet();
        var users = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToList();
        var userMap = users.ToDictionary(u => u.Id);

        return suppliers
            .Select(s =>
            {
                userMap.TryGetValue(s.UserId, out var user);
                capCountBySupplierId.TryGetValue(s.Id, out var capCount);
                return new AdminSupplierDto(
                    s.Id, s.UserId,
                    user?.Email ?? "",
                    user?.FullName ?? "",
                    s.CompanyName, s.Country, s.CompanySize,
                    capCount,
                    s.EmbeddingVector is { Length: > 0 },
                    user?.Tier ?? SubscriptionTier.Free,
                    user?.LockoutEnd > DateTimeOffset.UtcNow,
                    s.CreatedAt);
            })
            .OrderBy(d => d.CompanyName)
            .ToList();
    }
}
