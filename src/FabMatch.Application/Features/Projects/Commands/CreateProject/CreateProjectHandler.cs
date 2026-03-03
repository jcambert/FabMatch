using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Handles <see cref="CreateProjectCommand"/>:
/// validates the client exists, creates the project entity and persists it.
/// </summary>
public sealed class CreateProjectHandler : ICommandHandler<CreateProjectCommand, CreateProjectResult>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateProjectHandler> _logger;

    public CreateProjectHandler(IUnitOfWork uow, ILogger<CreateProjectHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async ValueTask<CreateProjectResult> Handle(CreateProjectCommand cmd, CancellationToken ct)
    {
        var client = await _uow.Clients.GetByIdAsync(cmd.ClientId, ct)
            ?? throw new KeyNotFoundException($"Client {cmd.ClientId} not found.");

        var project = Project.Create(
            client.Id,
            cmd.Title,
            cmd.Description,
            cmd.IsPublic,
            cmd.DeliveryDate,
            cmd.BudgetEstimate,
            cmd.BudgetCurrency);

        project.Update(
            cmd.Title, cmd.Description, cmd.IsPublic,
            cmd.DeliveryDate, cmd.BudgetEstimate, cmd.BudgetCurrency,
            cmd.RequiredProcesses, cmd.Materials);

        await _uow.Projects.AddAsync(project, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Project {ProjectId} created for client {ClientId}", project.Id, client.Id);
        return new CreateProjectResult(project.Id, project.Title);
    }
}
