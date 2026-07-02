using MediatR;

namespace Shared.Application.Mediators.Events;

/// <summary>
/// Base interface for all event-driven messages in the system.
/// Implements MediatR INotification.
/// </summary>
public interface IEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier for this specific event instance.
    /// </summary>
    string EventId { get; }

    /// <summary>
    /// Gets the UTC timestamp when this event originally occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the correlation identifier for tracing related message flows.
    /// </summary>
    string? CorrelationId { get; }
}
