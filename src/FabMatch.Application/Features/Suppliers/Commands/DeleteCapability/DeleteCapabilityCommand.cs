using Mediator;

namespace FabMatch.Application.Features.Suppliers.Commands.DeleteCapability;

/// <summary>Command to remove a production capability from a supplier's profile.</summary>
public sealed record DeleteCapabilityCommand(Guid CapabilityId, Guid SupplierId) : ICommand<bool>;
