using Mediator;

namespace FabMatch.Application.Features.Suppliers.Commands.CreateSupplier;

/// <summary>Command to create or complete a supplier profile for an existing user.</summary>
public sealed record CreateSupplierCommand(
    Guid UserId,
    string CompanyName,
    string Presentation,
    string Country,
    string? WebsiteUrl,
    int? FoundedYear,
    string? CompanySize,
    List<string>? Certifications,
    List<string>? Materials,
    decimal? AnnualCapacityTonnes) : ICommand<CreateSupplierResult>;

/// <summary>Result containing the new supplier profile ID.</summary>
public sealed record CreateSupplierResult(Guid SupplierId, string CompanyName);
