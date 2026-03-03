using Mediator;

namespace FabMatch.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Command to create a new manufacturing project for an authenticated client.
/// </summary>
public sealed record CreateProjectCommand(
    Guid ClientId,
    string Title,
    string Description,
    bool IsPublic,
    DateTime? DeliveryDate,
    decimal? BudgetEstimate,
    string BudgetCurrency,
    List<string>? RequiredProcesses,
    List<string>? Materials) : ICommand<CreateProjectResult>;

/// <summary>Result containing the ID of the newly created project.</summary>
public sealed record CreateProjectResult(Guid ProjectId, string Title);
