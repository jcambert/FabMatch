using FabMatch.Application.Common.Interfaces;
using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Suppliers.Commands.CreateSupplier;

/// <summary>
/// Creates a supplier profile and generates an initial AI embedding.
/// </summary>
public sealed class CreateSupplierHandler : ICommandHandler<CreateSupplierCommand, CreateSupplierResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IAIService _ai;
    private readonly ILogger<CreateSupplierHandler> _logger;

    public CreateSupplierHandler(IUnitOfWork uow, IAIService ai, ILogger<CreateSupplierHandler> logger)
    {
        _uow = uow;
        _ai = ai;
        _logger = logger;
    }

    public async ValueTask<CreateSupplierResult> Handle(CreateSupplierCommand cmd, CancellationToken ct)
    {
        var supplier = Supplier.Create(cmd.UserId, cmd.CompanyName, cmd.Presentation, cmd.Country, cmd.WebsiteUrl);
        supplier.Update(
            cmd.CompanyName, cmd.Presentation, cmd.Country, cmd.WebsiteUrl,
            cmd.FoundedYear, cmd.CompanySize, cmd.Certifications, cmd.Materials,
            cmd.AnnualCapacityTonnes, null, null);

        // Generate AI embedding for matching
        var embText = $"{cmd.CompanyName} {cmd.Presentation} {string.Join(" ", cmd.Materials ?? [])} {string.Join(" ", cmd.Certifications ?? [])}";
        var embedding = await _ai.GenerateEmbeddingAsync(embText, ct);
        supplier.SetEmbedding(embedding);

        await _uow.Suppliers.AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Supplier {SupplierId} created for user {UserId}", supplier.Id, cmd.UserId);
        return new CreateSupplierResult(supplier.Id, supplier.CompanyName);
    }
}
