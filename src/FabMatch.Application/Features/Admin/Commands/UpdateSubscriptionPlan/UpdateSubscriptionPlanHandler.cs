using FabMatch.Application.Common.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FabMatch.Application.Features.Admin.Commands.UpdateSubscriptionPlan;

public sealed class UpdateSubscriptionPlanHandler : ICommandHandler<UpdateSubscriptionPlanCommand, bool>
{
    private readonly ISubscriptionPlanRepository _plans;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<UpdateSubscriptionPlanHandler> _logger;

    public UpdateSubscriptionPlanHandler(
        ISubscriptionPlanRepository plans,
        IAuditLogger auditLogger,
        ILogger<UpdateSubscriptionPlanHandler> logger)
    {
        _plans = plans;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async ValueTask<bool> Handle(UpdateSubscriptionPlanCommand cmd, CancellationToken ct)
    {
        var config = await _plans.GetByTierAsync(cmd.Tier, ct)
            ?? throw new KeyNotFoundException($"Plan config for tier {cmd.Tier} not found.");

        config.MonthlyPrice        = cmd.MonthlyPrice;
        config.MaxProjects         = cmd.MaxProjects;
        config.MaxAnalysesPerMonth = cmd.MaxAnalysesPerMonth;
        config.UnlimitedMatching   = cmd.UnlimitedMatching;
        config.PrioritySupport     = cmd.PrioritySupport;
        config.Features            = cmd.Features.ToList();
        config.UpdatedAt           = DateTime.UtcNow;

        await _plans.SaveAsync(ct);

        _logger.LogInformation("Admin updated subscription plan {Tier}", cmd.Tier);

        await _auditLogger.LogAsync(
            "PlanUpdated",
            "SubscriptionPlan",
            Guid.Empty,
            cmd.Tier.ToString(),
            ct);

        return true;
    }
}
