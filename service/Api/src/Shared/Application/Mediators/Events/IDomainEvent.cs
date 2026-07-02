namespace Shared.Application.Mediators.Events;

/// <summary>
/// Represents a domain event that is raised when something significant 
/// happens within a domain entity or aggregate root.
/// </summary>
public interface IDomainEvent : IEvent;

/// <summary>
/// Represents a domain event that is raised when something significant 
/// happens within a domain entity or aggregate root.
/// </summary>
/// <typeparam name="TId">The type of the identifier of the entity or aggregate that raised this event.</typeparam>
public interface IDomainEvent<out TId> : IDomainEvent where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity or aggregate that raised this event.
    /// </summary>
    TId EntityId { get; }
}
