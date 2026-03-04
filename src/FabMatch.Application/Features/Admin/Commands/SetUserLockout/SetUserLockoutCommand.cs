using Mediator;

namespace FabMatch.Application.Features.Admin.Commands.SetUserLockout;

/// <summary>Admin command to lock or unlock a user account.</summary>
public sealed record SetUserLockoutCommand(Guid UserId, bool Locked) : ICommand<bool>;
