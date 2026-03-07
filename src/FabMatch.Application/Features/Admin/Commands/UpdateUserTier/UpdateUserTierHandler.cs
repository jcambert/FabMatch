using FabMatch.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Admin.Commands.UpdateUserTier;

public sealed class UpdateUserTierHandler : ICommandHandler<UpdateUserTierCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UpdateUserTierHandler> _logger;

    public UpdateUserTierHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateUserTierHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async ValueTask<bool> Handle(UpdateUserTierCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(cmd.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {cmd.UserId} not found.");

        var previous = user.Tier;
        user.Tier = cmd.NewTier;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation(
            "Admin changed tier for user {UserId} from {Previous} to {New}",
            cmd.UserId, previous, cmd.NewTier);

        return true;
    }
}
