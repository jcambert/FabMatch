using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FabMatch.Web.Controllers;

/// <summary>
/// Receives and processes Stripe webhook events for subscription lifecycle management.
/// Maps to <c>POST /api/webhooks/stripe</c>.
/// </summary>
/// <remarks>
/// This endpoint must be excluded from anti-forgery validation and HTTPS redirect so that
/// Stripe can reach it. The raw request body is passed to the payment service for signature
/// verification before any processing takes place.
/// </remarks>
[ApiController]
[Route("api/webhooks/stripe")]
public sealed class StripeWebhookController : ControllerBase
{
    private readonly IPaymentService _payment;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IPaymentService payment,
        UserManager<ApplicationUser> userManager,
        IEmailService email,
        ILogger<StripeWebhookController> logger)
    {
        _payment = payment;
        _userManager = userManager;
        _email = email;
        _logger = logger;
    }

    /// <summary>Handles incoming Stripe webhook events.</summary>
    [HttpPost]
    public async Task<IActionResult> HandleAsync(CancellationToken ct)
    {
        // 1. Read the raw body (required for signature verification)
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        if (!Request.Headers.TryGetValue("Stripe-Signature", out var signature))
        {
            _logger.LogWarning("Stripe webhook request missing Stripe-Signature header.");
            return BadRequest("Missing Stripe-Signature header.");
        }

        // 2. Verify the webhook signature
        Application.Common.Models.WebhookEvent evt;
        try
        {
            evt = await _payment.VerifyWebhookAsync(payload, signature!, ct);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return BadRequest("Invalid signature.");
        }

        _logger.LogInformation(
            "Stripe webhook received: EventId={EventId} Type={Type}",
            evt.EventId, evt.EventType);

        // 3. Route to the appropriate handler
        await (evt.EventType switch
        {
            "customer.subscription.updated" => HandleSubscriptionUpdatedAsync(evt, ct),
            "customer.subscription.deleted" => HandleSubscriptionDeletedAsync(evt, ct),
            "invoice.payment_succeeded"     => HandleInvoiceSucceededAsync(evt, ct),
            "invoice.payment_failed"        => HandleInvoiceFailedAsync(evt, ct),
            _                               => Task.CompletedTask
        });

        return Ok();
    }

    // ── Private event handlers ───────────────────────────────────────────────

    /// <summary>
    /// Handles <c>customer.subscription.updated</c>: upgrades or downgrades the user's tier
    /// based on the subscription status reported by Stripe.
    /// </summary>
    private async Task HandleSubscriptionUpdatedAsync(
        Application.Common.Models.WebhookEvent evt, CancellationToken ct)
    {
        // Parse the customer ID from the raw JSON payload
        using var doc = System.Text.Json.JsonDocument.Parse(evt.RawPayload);
        var dataObj = doc.RootElement.GetProperty("data").GetProperty("object");

        var customerId = dataObj.TryGetProperty("customer", out var custEl)
            ? custEl.GetString() : null;
        var status = dataObj.TryGetProperty("status", out var statEl)
            ? statEl.GetString() : null;

        if (string.IsNullOrEmpty(customerId)) return;

        var user = await FindUserByCustomerIdAsync(customerId, ct);
        if (user is null) return;

        // Downgrade to Free if the subscription is no longer active
        if (status is "canceled" or "unpaid" or "past_due")
        {
            user.Tier = SubscriptionTier.Free;
            await _userManager.UpdateAsync(user);
            _logger.LogInformation(
                "User {UserId} downgraded to Free (subscription status: {Status})",
                user.Id, status);
        }
    }

    /// <summary>Handles <c>customer.subscription.deleted</c>: resets the user's tier to Free.</summary>
    private async Task HandleSubscriptionDeletedAsync(
        Application.Common.Models.WebhookEvent evt, CancellationToken ct)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(evt.RawPayload);
        var customerId = doc.RootElement
            .GetProperty("data")
            .GetProperty("object")
            .TryGetProperty("customer", out var el) ? el.GetString() : null;

        if (string.IsNullOrEmpty(customerId)) return;

        var user = await FindUserByCustomerIdAsync(customerId, ct);
        if (user is null) return;

        user.Tier = SubscriptionTier.Free;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Subscription deleted for user {UserId}; tier reset to Free.", user.Id);
    }

    /// <summary>Handles <c>invoice.payment_succeeded</c>: confirms subscription payment to the user.</summary>
    private async Task HandleInvoiceSucceededAsync(
        Application.Common.Models.WebhookEvent evt, CancellationToken ct)
    {
        _logger.LogInformation("Invoice payment succeeded for resource {ResourceId}.", evt.ResourceId);

        using var doc = System.Text.Json.JsonDocument.Parse(evt.RawPayload);
        var dataObj = doc.RootElement.GetProperty("data").GetProperty("object");
        var customerId = dataObj.TryGetProperty("customer", out var custEl) ? custEl.GetString() : null;
        var amountPaid = dataObj.TryGetProperty("amount_paid", out var amtEl) ? amtEl.GetInt64() / 100m : 0m;
        var currency = dataObj.TryGetProperty("currency", out var currEl) ? currEl.GetString()?.ToUpper() : "EUR";

        if (string.IsNullOrEmpty(customerId)) return;
        var user = await FindUserByCustomerIdAsync(customerId, ct);
        if (user?.Email is null) return;

        await _email.SendAsync(
            user.Email,
            user.FullName,
            "Payment confirmation – FabMatch",
            $"<p>Hi {user.FullName},</p>" +
            $"<p>Your payment of <strong>{amountPaid:N2} {currency}</strong> has been processed successfully.</p>" +
            "<p>Thank you for your subscription to FabMatch.</p>",
            ct: ct);
    }

    /// <summary>Handles <c>invoice.payment_failed</c>: notifies user of the payment failure.</summary>
    private async Task HandleInvoiceFailedAsync(
        Application.Common.Models.WebhookEvent evt, CancellationToken ct)
    {
        _logger.LogWarning(
            "Invoice payment FAILED for resource {ResourceId}. Manual review may be required.",
            evt.ResourceId);

        using var doc = System.Text.Json.JsonDocument.Parse(evt.RawPayload);
        var dataObj = doc.RootElement.GetProperty("data").GetProperty("object");
        var customerId = dataObj.TryGetProperty("customer", out var custEl) ? custEl.GetString() : null;

        if (string.IsNullOrEmpty(customerId)) return;
        var user = await FindUserByCustomerIdAsync(customerId, ct);
        if (user?.Email is null) return;

        await _email.SendAsync(
            user.Email,
            user.FullName,
            "Payment failed – action required",
            $"<p>Hi {user.FullName},</p>" +
            "<p>We were unable to process your latest payment. Please update your payment method to avoid service interruption.</p>" +
            "<p><a href=\"/subscription\">Manage your subscription</a></p>",
            ct: ct);
    }

    /// <summary>Looks up a user by their Stripe customer ID.</summary>
    private async Task<ApplicationUser?> FindUserByCustomerIdAsync(
        string customerId, CancellationToken ct)
    {
        var user = _userManager.Users
            .FirstOrDefault(u => u.StripeCustomerId == customerId);

        if (user is null)
            _logger.LogWarning("No user found for Stripe customer {CustomerId}.", customerId);

        return await Task.FromResult(user);
    }
}
