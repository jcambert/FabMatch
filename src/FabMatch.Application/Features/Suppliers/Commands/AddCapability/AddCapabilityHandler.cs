using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Suppliers.Commands.AddCapability;

/// <summary>
/// Creates a new <see cref="ProductionCapability"/> entry for the given supplier
/// and invalidates the supplier's AI embedding so it gets regenerated on next match run.
/// </summary>
public sealed class AddCapabilityHandler : ICommandHandler<AddCapabilityCommand, AddCapabilityResult>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AddCapabilityHandler> _logger;

    public AddCapabilityHandler(IUnitOfWork uow, ILogger<AddCapabilityHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async ValueTask<AddCapabilityResult> Handle(AddCapabilityCommand cmd, CancellationToken ct)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(cmd.SupplierId, ct)
            ?? throw new KeyNotFoundException($"Supplier {cmd.SupplierId} not found.");

        var capability = ProductionCapability.Create(
            cmd.SupplierId,
            cmd.ProcessType,
            cmd.EquipmentName,
            cmd.MaxThicknessMm,
            cmd.MaxDimensionXMm,
            cmd.MaxDimensionYMm,
            cmd.ToleranceMm,
            cmd.Notes);

        await _uow.Capabilities.AddAsync(capability, ct);

        // Invalidate embedding so it gets regenerated at next matching run
        supplier.SetEmbedding(Array.Empty<float>());
        _uow.Suppliers.Update(supplier);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Capability '{ProcessType}' added to supplier {SupplierId}", cmd.ProcessType, cmd.SupplierId);

        return new AddCapabilityResult(capability.Id, capability.ProcessType);
    }
}
