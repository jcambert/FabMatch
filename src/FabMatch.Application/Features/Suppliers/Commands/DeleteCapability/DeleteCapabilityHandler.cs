using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Suppliers.Commands.DeleteCapability;

/// <summary>Soft-deletes a production capability and invalidates the supplier's embedding.</summary>
public sealed class DeleteCapabilityHandler : ICommandHandler<DeleteCapabilityCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DeleteCapabilityHandler> _logger;

    public DeleteCapabilityHandler(IUnitOfWork uow, ILogger<DeleteCapabilityHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async ValueTask<bool> Handle(DeleteCapabilityCommand cmd, CancellationToken ct)
    {
        var capability = await _uow.Capabilities.GetByIdAsync(cmd.CapabilityId, ct)
            ?? throw new KeyNotFoundException($"Capability {cmd.CapabilityId} not found.");

        if (capability.SupplierId != cmd.SupplierId)
            throw new UnauthorizedAccessException("Capability does not belong to this supplier.");

        capability.Delete();
        _uow.Capabilities.Update(capability);

        var supplier = await _uow.Suppliers.GetByIdAsync(cmd.SupplierId, ct);
        if (supplier is not null)
        {
            supplier.SetEmbedding(Array.Empty<float>());
            _uow.Suppliers.Update(supplier);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Capability {Id} deleted from supplier {SupplierId}", cmd.CapabilityId, cmd.SupplierId);
        return true;
    }
}
