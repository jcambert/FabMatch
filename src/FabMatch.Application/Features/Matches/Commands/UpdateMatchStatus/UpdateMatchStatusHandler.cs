using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Enums;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Matches.Commands.UpdateMatchStatus;

/// <summary>
/// Applies a status transition to a <see cref="Domain.Entities.Match"/>
/// and notifies the other party in real-time via SignalR and email.
/// </summary>
public sealed class UpdateMatchStatusHandler : ICommandHandler<UpdateMatchStatusCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationHubService _hub;
    private readonly IEmailService _email;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UpdateMatchStatusHandler> _logger;

    public UpdateMatchStatusHandler(
        IUnitOfWork uow,
        INotificationHubService hub,
        IEmailService email,
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateMatchStatusHandler> logger)
    {
        _uow = uow;
        _hub = hub;
        _email = email;
        _userManager = userManager;
        _logger = logger;
    }

    public async ValueTask<bool> Handle(UpdateMatchStatusCommand cmd, CancellationToken ct)
    {
        var match = await _uow.Matches.GetByIdAsync(cmd.MatchId, ct)
            ?? throw new KeyNotFoundException($"Match {cmd.MatchId} not found.");

        var project = await _uow.Projects.GetByIdAsync(match.ProjectId, ct)!;
        var supplier = await _uow.Suppliers.GetByIdAsync(match.SupplierId, ct)!;

        switch (cmd.NewStatus)
        {
            case MatchStatus.AcceptedByClient:
                match.AcceptByClient();
                await NotifyAsync(supplier!.UserId, NotificationType.MatchAccepted,
                    "Match Accepted", $"A client accepted your match for project '{project!.Title}'.",
                    $"/matches/{match.Id}", ct);
                await EmailMatchAsync(supplier.UserId,
                    $"Match accepted – {project.Title}",
                    $"Good news! A client has accepted your match proposal for project <strong>{project.Title}</strong>.",
                    $"/matches/{match.Id}", ct);
                break;

            case MatchStatus.RejectedByClient:
                match.RejectByClient();
                await NotifyAsync(supplier!.UserId, NotificationType.MatchRejected,
                    "Match Rejected", "A client rejected your match proposal.", null, ct);
                break;

            case MatchStatus.AcceptedBySupplier:
                match.AcceptBySupplier();
                await NotifyAsync(project!.Client.UserId, NotificationType.MatchAccepted,
                    "Supplier Accepted", $"Supplier '{supplier!.CompanyName}' accepted your project match.",
                    $"/projects/{project.Id}/matches", ct);
                await EmailMatchAsync(project.Client.UserId,
                    $"Supplier accepted – {project.Title}",
                    $"Supplier <strong>{supplier.CompanyName}</strong> has accepted the match for your project <strong>{project.Title}</strong>.",
                    $"/projects/{project.Id}/matches", ct);
                break;

            case MatchStatus.RejectedBySupplier:
                match.RejectBySupplier();
                await NotifyAsync(project!.Client.UserId, NotificationType.MatchRejected,
                    "Supplier Declined", $"Supplier '{supplier!.CompanyName}' declined your project match.",
                    $"/projects/{project.Id}/matches", ct);
                break;

            case MatchStatus.Finalised:
                match.Finalise();
                project!.Award();
                break;

            default:
                throw new InvalidOperationException($"Unsupported status transition: {cmd.NewStatus}");
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Match {MatchId} status updated to {Status}", match.Id, cmd.NewStatus);
        return true;
    }

    private async Task NotifyAsync(
        Guid userId, NotificationType type, string title, string message,
        string? actionUrl, CancellationToken ct)
    {
        await _hub.SendToUserAsync(userId, type, title, message, actionUrl, ct);
        var notif = Domain.Entities.Notification.Create(userId, type, title, message, actionUrl);
        await _uow.Notifications.AddAsync(notif, ct);
    }

    private async Task EmailMatchAsync(
        Guid userId, string subject, string bodyHtml, string actionUrl, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user?.Email is null) return;

        await _email.SendAsync(
            user.Email,
            user.FullName,
            subject,
            $"<p>Hi {user.FullName},</p><p>{bodyHtml}</p>" +
            $"<p><a href=\"{actionUrl}\">View details</a></p>",
            ct: ct);
    }
}
