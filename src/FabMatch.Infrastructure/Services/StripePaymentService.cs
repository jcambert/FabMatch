using FabMatch.Application.Common.Interfaces;
using FabMatch.Application.Common.Models;
using FabMatch.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// Stripe-based implementation of <see cref="IPaymentService"/>.
/// </summary>
public sealed class StripePaymentService : IPaymentService
{
    private readonly StripeOptions _opts;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IOptions<StripeOptions> opts, ILogger<StripePaymentService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _opts.SecretKey;
    }

    /// <inheritdoc />
    public async Task<string> EnsureCustomerAsync(
        Guid userId, string email, string fullName, CancellationToken ct = default)
    {
        var service = new CustomerService();
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = fullName,
            Metadata = new Dictionary<string, string> { ["userId"] = userId.ToString() }
        };
        var customer = await service.CreateAsync(options, cancellationToken: ct);
        _logger.LogDebug("Stripe customer {CustomerId} created for user {UserId}", customer.Id, userId);
        return customer.Id;
    }

    /// <inheritdoc />
    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        string customerId,
        MoneyAmount amount,
        string description,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        var service = new PaymentIntentService();
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount.Amount * 100), // Convert to cents
            Currency = amount.Currency.ToLowerInvariant(),
            Customer = customerId,
            Description = description,
            Metadata = metadata?.ToDictionary(k => k.Key, v => v.Value) ?? []
        };

        var intent = await service.CreateAsync(options, cancellationToken: ct);
        return new PaymentIntentResult(intent.Id, intent.ClientSecret, intent.Amount, intent.Currency);
    }

    /// <inheritdoc />
    public async Task<SubscriptionResult> CreateSubscriptionAsync(
        string customerId, string priceId, CancellationToken ct = default)
    {
        var service = new SubscriptionService();
        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = [new SubscriptionItemOptions { Price = priceId }],
            PaymentBehavior = "default_incomplete",
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription"
            },
            Expand = ["latest_invoice.payment_intent"]
        };

        var subscription = await service.CreateAsync(options, cancellationToken: ct);
        var clientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret;
        return new SubscriptionResult(subscription.Id, subscription.Status, clientSecret);
    }

    /// <inheritdoc />
    public async Task CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        var service = new SubscriptionService();
        await service.UpdateAsync(subscriptionId,
            new SubscriptionUpdateOptions { CancelAtPeriodEnd = true },
            cancellationToken: ct);
        _logger.LogInformation("Subscription {SubId} set to cancel at period end", subscriptionId);
    }

    /// <inheritdoc />
    public Task<WebhookEvent> VerifyWebhookAsync(
        string payload, string signature, CancellationToken ct = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _opts.WebhookSecret);
            var resourceId = stripeEvent.Data?.Object is IHasId hasId ? hasId.Id : string.Empty;
            var result = new WebhookEvent(
                stripeEvent.Id,
                stripeEvent.Type,
                resourceId,
                payload);
            return Task.FromResult(result);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException("Invalid Stripe webhook signature.", ex);
        }
    }
}

/// <summary>Configuration options for Stripe integration.</summary>
public sealed class StripeOptions
{
    public const string Section = "Payment:Stripe";

    /// <summary>Stripe secret key (sk_live_… or sk_test_…).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Stripe publishable key for the frontend.</summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>Webhook signing secret for verifying Stripe events.</summary>
    public string WebhookSecret { get; set; } = string.Empty;
}
