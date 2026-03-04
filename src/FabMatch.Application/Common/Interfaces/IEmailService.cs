namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Abstraction over an SMTP / transactional email provider.
/// Default implementation uses MailKit; swap via DI for SendGrid, SES, etc.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends a plain-text + HTML email.</summary>
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default);

    /// <summary>Sends a password reset email containing the reset link.</summary>
    Task SendPasswordResetAsync(string toEmail, string toName, string resetLink, CancellationToken ct = default);

    /// <summary>Sends an email confirmation link to a newly registered user.</summary>
    Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmLink, CancellationToken ct = default);

    /// <summary>Sends a new-match notification email to a user.</summary>
    Task SendMatchNotificationAsync(
        string toEmail,
        string toName,
        string projectTitle,
        string counterpartyName,
        string matchLink,
        CancellationToken ct = default);
}
