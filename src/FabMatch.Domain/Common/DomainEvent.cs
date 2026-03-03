namespace FabMatch.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// Implementations are dispatched via the Mediator pipeline.
/// </summary>
public interface IDomainEvent { }

/// <summary>
/// Base record for all domain events, carrying a unique event ID and timestamp.
/// </summary>
/// <param name="EventId">Unique identifier for the event.</param>
/// <param name="OccurredOn">UTC timestamp when the event occurred.</param>
public abstract record DomainEvent(Guid EventId, DateTime OccurredOn) : IDomainEvent
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}

/// <summary>
/// Raised when a new Match between a client project and a supplier is created.
/// </summary>
/// <param name="MatchId">The ID of the created match.</param>
/// <param name="ProjectId">The project for which the match was created.</param>
/// <param name="SupplierId">The matched supplier.</param>
/// <param name="ClientUserId">The owner (client user) of the project.</param>
public sealed record MatchCreatedEvent(
    Guid MatchId,
    Guid ProjectId,
    Guid SupplierId,
    Guid ClientUserId) : DomainEvent;

/// <summary>
/// Raised when a plan has been successfully analysed by the AI service.
/// </summary>
/// <param name="PlanId">The plan that was analysed.</param>
/// <param name="ProjectId">Owning project.</param>
public sealed record PlanAnalysedEvent(Guid PlanId, Guid ProjectId) : DomainEvent;

/// <summary>
/// Raised when a payment has been successfully processed.
/// </summary>
/// <param name="PaymentId">The processed payment.</param>
/// <param name="UserId">The user who made the payment.</param>
public sealed record PaymentProcessedEvent(Guid PaymentId, Guid UserId) : DomainEvent;
