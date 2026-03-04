using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Payments.Commands.CreateSubscription;

/// <summary>
/// Ensures a Stripe customer exists for the user, then creates a Stripe subscription
/// for the requested tier. Updates the user's <see cref="ApplicationUser.Tier"/> and
/// <see cref="ApplicationUser.StripeCustomerId"/> on success.
/// </summary>
public sealed class CreateSubscriptionHandler
    : ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentService _payment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateSubscriptionHandler> _logger;

    public CreateSubscriptionHandler(
        UserManager<ApplicationUser> userManager,
        IPaymentService payment,
        IConfiguration configuration,
        ILogger<CreateSubscriptionHandler> logger)
    {
        _userManager = userManager;
        _payment = payment;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<CreateSubscriptionResult> Handle(
        CreateSubscriptionCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(cmd.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {cmd.UserId} not found.");

        // 1. Ensure Stripe customer
        if (string.IsNullOrEmpty(user.StripeCustomerId))
        {
            user.StripeCustomerId = await _payment.EnsureCustomerAsync(
                cmd.UserId, cmd.UserEmail, cmd.UserFullName, ct);
            await _userManager.UpdateAsync(user);
        }

        // 2. Look up the Stripe price ID for the requested tier
        var priceId = _configuration[$"Payment:Stripe:PriceIds:{cmd.TargetTier}"]
            ?? throw new InvalidOperationException(
                $"No Stripe price ID configured for tier '{cmd.TargetTier}'. " +
                "Add 'Payment:Stripe:PriceIds:<Tier>' to appsettings.");

        // 3. Create the Stripe subscription
        var sub = await _payment.CreateSubscriptionAsync(user.StripeCustomerId, priceId, ct);

        // 4. Update the user's tier in the database
        user.Tier = cmd.TargetTier;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation(
            "User {UserId} subscribed to tier {Tier}. SubscriptionId={SubId}",
            cmd.UserId, cmd.TargetTier, sub.SubscriptionId);

        return new CreateSubscriptionResult(sub.SubscriptionId, sub.ClientSecret, sub.Status);
    }
}
