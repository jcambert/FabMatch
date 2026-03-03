using FluentValidation;
using Mediator;

namespace FabMatch.Application.Common.Behaviors;

/// <summary>
/// Mediator pipeline behavior that runs all registered FluentValidation validators
/// before the handler is invoked.  Throws <see cref="ValidationException"/> on failure.
/// </summary>
/// <typeparam name="TMessage">The request/command/query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
        => _validators = validators;

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken ct,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (!_validators.Any())
            return await next(message, ct);

        var context = new ValidationContext<TMessage>(message);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next(message, ct);
    }
}
