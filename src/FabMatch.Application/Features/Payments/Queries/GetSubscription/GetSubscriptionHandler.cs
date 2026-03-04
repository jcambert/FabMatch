using FabMatch.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace FabMatch.Application.Features.Payments.Queries.GetSubscription;

/// <summary>
/// Returns the subscription status for a user by loading their Identity record.
/// </summary>
public sealed class GetSubscriptionHandler
    : IQueryHandler<GetSubscriptionQuery, SubscriptionInfoDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetSubscriptionHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    /// <inheritdoc />
    public async ValueTask<SubscriptionInfoDto> Handle(GetSubscriptionQuery query, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {query.UserId} not found.");

        return new SubscriptionInfoDto(
            user.Tier,
            user.StripeCustomerId,
            user.StripeCustomerId is not null);
    }
}
