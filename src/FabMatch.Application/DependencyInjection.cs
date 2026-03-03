using FabMatch.Application.Common.Behaviors;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace FabMatch.Application;

/// <summary>
/// Registers all Application-layer services with the DI container.
/// Call this in <c>Program.cs</c>: <c>builder.Services.AddApplication();</c>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── Mediator (Martin Othamar – source-generator based) ─────────────────
        // Mediator.SourceGenerator generates the IMediator implementation at compile-time.
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // ── Pipeline behaviors ─────────────────────────────────────────────────
        // Order matters: Logging → Validation → Handler
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // ── FluentValidation ───────────────────────────────────────────────────
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
