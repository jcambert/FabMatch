using FluentValidation;

namespace FabMatch.Application.Features.Projects.Commands.CreateProject;

/// <summary>Validates <see cref="CreateProjectCommand"/>.</summary>
public sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.BudgetCurrency).Length(3).When(x => x.BudgetEstimate.HasValue);
        RuleFor(x => x.BudgetEstimate).GreaterThan(0).When(x => x.BudgetEstimate.HasValue);
        RuleFor(x => x.DeliveryDate).GreaterThan(DateTime.UtcNow).When(x => x.DeliveryDate.HasValue);
    }
}
