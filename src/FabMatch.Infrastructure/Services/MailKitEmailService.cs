using FabMatch.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// MailKit-based implementation of <see cref="IEmailService"/>.
/// Sends emails via SMTP with optional STARTTLS/SSL.
/// </summary>
public sealed class MailKitEmailService : IEmailService
{
    private readonly SmtpOptions _opts;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IOptions<SmtpOptions> opts, ILogger<MailKitEmailService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(
        string toEmail, string toName, string subject,
        string htmlBody, string? plainTextBody = null,
        CancellationToken ct = default)
    {
        var message = BuildMessage(toEmail, toName, subject, htmlBody, plainTextBody);
        await SendMessageAsync(message, ct);
    }

    /// <inheritdoc />
    public Task SendPasswordResetAsync(
        string toEmail, string toName, string resetLink, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Reset your FabMatch password</h2>
            <p>Hello {toName},</p>
            <p>Click the button below to reset your password. This link expires in 24 hours.</p>
            <p style="margin:24px 0;">
                <a href="{resetLink}"
                   style="background:#1565C0;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                   Reset Password
                </a>
            </p>
            <p>If you did not request a password reset, ignore this email.</p>
            <p>— The FabMatch Team</p>
            """;
        return SendAsync(toEmail, toName, "Reset your FabMatch password", html, ct: ct);
    }

    /// <inheritdoc />
    public Task SendEmailConfirmationAsync(
        string toEmail, string toName, string confirmLink, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Confirm your FabMatch account</h2>
            <p>Hello {toName},</p>
            <p>Please confirm your email address by clicking the button below.</p>
            <p style="margin:24px 0;">
                <a href="{confirmLink}"
                   style="background:#1565C0;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                   Confirm Email
                </a>
            </p>
            <p>— The FabMatch Team</p>
            """;
        return SendAsync(toEmail, toName, "Confirm your FabMatch email", html, ct: ct);
    }

    /// <inheritdoc />
    public Task SendMatchNotificationAsync(
        string toEmail, string toName, string projectTitle,
        string counterpartyName, string matchLink, CancellationToken ct = default)
    {
        var html = $"""
            <h2>New Match on FabMatch!</h2>
            <p>Hello {toName},</p>
            <p>A new match has been proposed between <strong>{projectTitle}</strong>
               and <strong>{counterpartyName}</strong>.</p>
            <p style="margin:24px 0;">
                <a href="{matchLink}"
                   style="background:#F57C00;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">
                   Review Match
                </a>
            </p>
            <p>— The FabMatch Team</p>
            """;
        return SendAsync(toEmail, toName, $"New match: {projectTitle}", html, ct: ct);
    }

    // ── Private helpers ────────────────────────────────────────────

    private MimeMessage BuildMessage(
        string toEmail, string toName, string subject,
        string htmlBody, string? plainText)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_opts.FromName, _opts.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = plainText ?? HtmlToPlainText(htmlBody)
        };
        message.Body = builder.ToMessageBody();
        return message;
    }

    private async Task SendMessageAsync(MimeMessage message, CancellationToken ct)
    {
        if (_opts.UseNullSink)
        {
            _logger.LogInformation(
                "Email (null sink) to {To}: {Subject}", message.To, message.Subject);
            return;
        }

        using var client = new SmtpClient();
        try
        {
            var secureOpts = _opts.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_opts.Host, _opts.Port, secureOpts, ct);
            if (!string.IsNullOrEmpty(_opts.Username))
                await client.AuthenticateAsync(_opts.Username, _opts.Password, ct);

            await client.SendAsync(message, ct);
            _logger.LogInformation("Email sent to {To}: {Subject}", message.To, message.Subject);
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }

    private static string HtmlToPlainText(string html)
        => System.Text.RegularExpressions.Regex
            .Replace(html, "<[^>]*>", " ")
            .Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">")
            .Trim();
}

/// <summary>SMTP configuration options.</summary>
public sealed class SmtpOptions
{
    public const string Section = "Email:Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = false;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "noreply@fabmatch.io";
    public string FromName { get; set; } = "FabMatch";

    /// <summary>When true, emails are logged but not sent (useful for development).</summary>
    public bool UseNullSink { get; set; } = true;
}
