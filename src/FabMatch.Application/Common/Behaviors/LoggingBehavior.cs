using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Common.Behaviors;

/// <summary>
/// Mediator pipeline behavior that logs entry and exit for every request,
/// including elapsed time and any exceptions.
/// </summary>
/// <typeparam name="TMessage">The request/command/query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger<LoggingBehavior<TMessage, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
        => _logger = logger;

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken ct,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var messageName = typeof(TMessage).Name;
        _logger.LogInformation("Handling {MessageName}", messageName);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await next(message, ct);
            sw.Stop();
            _logger.LogInformation(
                "Handled {MessageName} in {ElapsedMs} ms",
                messageName,
                sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Error handling {MessageName} after {ElapsedMs} ms",
                messageName,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
