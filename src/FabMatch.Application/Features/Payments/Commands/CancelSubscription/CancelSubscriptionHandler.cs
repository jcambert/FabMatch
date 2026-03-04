using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Payments.Commands.CancelSubscription;

/// <summary>
/// Cancels the Stripe subscription and downgrades the user's tier to Free.
/// </summary>
public sealed class CancelSubscriptionHandler : ICommandHandler<CancelSubscriptionCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentService _payment;
    private readonly ILogger<CancelSubscriptionHandler> _logger;

    public CancelSubscriptionHandler(
        UserManager<ApplicationUser> userManager,
        IPaymentService payment,
        ILogger<CancelSubscriptionHandler> logger)
    {
        _userManager = userManager;
        _payment = payment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<Unit> Handle(CancelSubscriptionCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(cmd.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {cmd.UserId} not found.");

        await _payment.CancelSubscriptionAsync(cmd.SubscriptionId, ct);

        user.Tier = SubscriptionTier.Free;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation(
            "User {UserId} cancelled subscription {SubId}; tier reset to Free.",
            cmd.UserId, cmd.SubscriptionId);

        return Unit.Value;
    }
}
