using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace FabMatch.Application.Features.Admin.Queries.GetAllClients;

public sealed class GetAllClientsAdminHandler
    : IQueryHandler<GetAllClientsAdminQuery, IReadOnlyList<AdminClientDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllClientsAdminHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async ValueTask<IReadOnlyList<AdminClientDto>> Handle(
        GetAllClientsAdminQuery query, CancellationToken ct)
    {
        var clients = await _uow.Clients.GetAllAsync(ct);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLower();
            clients = clients.Where(c =>
                c.CompanyName.ToLower().Contains(term) ||
                c.Industry.ToLower().Contains(term) ||
                (c.Description ?? "").ToLower().Contains(term)).ToList();
        }

        var userIds = clients.Select(c => c.UserId).ToHashSet();
        var users = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToList();
        var userMap = users.ToDictionary(u => u.Id);

        return clients
            .Select(c =>
            {
                userMap.TryGetValue(c.UserId, out var user);
                return new AdminClientDto(
                    c.Id, c.UserId,
                    user?.Email ?? "",
                    user?.FullName ?? "",
                    c.CompanyName, c.Industry, c.WebsiteUrl, c.CompanySize,
                    user?.Tier ?? SubscriptionTier.Free,
                    user?.LockoutEnd > DateTimeOffset.UtcNow,
                    c.CreatedAt);
            })
            .OrderBy(d => d.CompanyName)
            .ToList();
    }
}
