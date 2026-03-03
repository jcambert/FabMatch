using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Matches.Commands.UpdateMatchStatus;

/// <summary>Command to advance the status of an existing match.</summary>
/// <param name="MatchId">The match to update.</param>
/// <param name="NewStatus">Desired new status.</param>
/// <param name="ActorUserId">The user performing the action (for authorization checks).</param>
public sealed record UpdateMatchStatusCommand(
    Guid MatchId,
    MatchStatus NewStatus,
    Guid ActorUserId) : ICommand<bool>;
