using FabMatch.Domain.Entities;
using FabMatch.Domain.Interfaces;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Auth.Commands.RegisterUser;

/// <summary>
/// Handles <see cref="RegisterUserCommand"/>:
/// 1. Creates an ASP.NET Core Identity user.
/// 2. Assigns client/supplier roles as requested.
/// 3. Creates corresponding profile entities via the Unit of Work.
/// </summary>
public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork uow,
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _uow = uow;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<RegisterUserResult> Handle(
        RegisterUserCommand cmd,
        CancellationToken ct)
    {
        // 1. Create Identity user
        var user = new ApplicationUser
        {
            UserName = cmd.Email,
            Email = cmd.Email,
            FirstName = cmd.FirstName,
            LastName = cmd.LastName
        };

        var identityResult = await _userManager.CreateAsync(user, cmd.Password);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", cmd.Email, errors);
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        // 2. Assign roles and create profiles
        if (cmd.RegisterAsClient)
        {
            await _userManager.AddToRoleAsync(user, "Client");
            var client = Client.Create(user.Id, cmd.CompanyName!, "General", "");
            await _uow.Clients.AddAsync(client, ct);
        }

        if (cmd.RegisterAsSupplier)
        {
            await _userManager.AddToRoleAsync(user, "Supplier");
            var supplier = Supplier.Create(user.Id, cmd.CompanyName!, "", "");
            await _uow.Suppliers.AddAsync(supplier, ct);
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} registered successfully", user.Id);
        return new RegisterUserResult(user.Id, user.Email!);
    }
}
