using Mediator;

namespace FabMatch.Application.Features.Auth.Commands.RegisterUser;

/// <summary>
/// Command to register a new platform user.
/// The user can optionally create a client profile, a supplier profile, or both.
/// </summary>
/// <param name="Email">Valid email address (used as login).</param>
/// <param name="Password">Plain-text password (hashed by Identity).</param>
/// <param name="FirstName">User's given name.</param>
/// <param name="LastName">User's family name.</param>
/// <param name="RegisterAsClient">Creates a client profile when <c>true</c>.</param>
/// <param name="RegisterAsSupplier">Creates a supplier profile when <c>true</c>.</param>
/// <param name="CompanyName">Required when registering as client or supplier.</param>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    bool RegisterAsClient,
    bool RegisterAsSupplier,
    string? CompanyName) : ICommand<RegisterUserResult>;

/// <summary>Result returned after successful registration.</summary>
/// <param name="UserId">The newly created user's ID.</param>
/// <param name="Email">User's email address.</param>
public sealed record RegisterUserResult(Guid UserId, string Email);
