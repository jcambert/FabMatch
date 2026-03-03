using FluentValidation;

namespace FabMatch.Application.Features.Auth.Commands.RegisterUser;

/// <summary>Validates <see cref="RegisterUserCommand"/> inputs.</summary>
public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required when registering as client or supplier.")
            .When(x => x.RegisterAsClient || x.RegisterAsSupplier);

        RuleFor(x => x)
            .Must(x => x.RegisterAsClient || x.RegisterAsSupplier)
            .WithMessage("User must register as at least a Client or a Supplier.")
            .WithName("Role");
    }
}
