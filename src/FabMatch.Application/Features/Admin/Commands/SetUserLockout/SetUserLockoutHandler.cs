using FabMatch.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Admin.Commands.SetUserLockout;

/// <summary>Locks or unlocks a user account via ASP.NET Core Identity.</summary>
public sealed class SetUserLockoutHandler : ICommandHandler<SetUserLockoutCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SetUserLockoutHandler> _logger;

    public SetUserLockoutHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<SetUserLockoutHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async ValueTask<bool> Handle(SetUserLockoutCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(cmd.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {cmd.UserId} not found.");

        var lockoutEnd = cmd.Locked ? DateTimeOffset.UtcNow.AddYears(100) : (DateTimeOffset?)null;
        await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        _logger.LogInformation("User {Id} {Action} by admin", cmd.UserId, cmd.Locked ? "locked" : "unlocked");
        return true;
    }
}
