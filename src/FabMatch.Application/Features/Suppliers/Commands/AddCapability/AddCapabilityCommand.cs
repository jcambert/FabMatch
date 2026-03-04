using Mediator;

namespace FabMatch.Application.Features.Suppliers.Commands.AddCapability;

/// <summary>
/// Command to add a production capability (machine/process) to a supplier's profile.
/// </summary>
public sealed record AddCapabilityCommand(
    Guid SupplierId,
    string ProcessType,
    string? EquipmentName,
    decimal? MaxThicknessMm,
    decimal? MaxDimensionXMm,
    decimal? MaxDimensionYMm,
    decimal? ToleranceMm,
    string? Notes) : ICommand<AddCapabilityResult>;

/// <summary>Result containing the new capability ID.</summary>
public sealed record AddCapabilityResult(Guid CapabilityId, string ProcessType);
