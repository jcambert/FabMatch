using FabMatch.Domain.Enums;
using Mediator;

namespace FabMatch.Application.Features.Admin.Commands.UpdateUserTier;

/// <summary>Admin command: manually changes a user's subscription tier.</summary>
public sealed record UpdateUserTierCommand(Guid UserId, SubscriptionTier NewTier) : ICommand<bool>;
