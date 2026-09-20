namespace Shared.Application.Domain.Concerns.Entities;

/// <summary>
/// Defines the basic contract for all domain entities with a unique identifier.
/// Entities are objects that have a distinct identity that persists over time.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
// Contract: pre=TId is equatable, post=Id is unique and immutable
public interface IEntity<out TId>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    // AgentHint: do NOT use setter in implementations; identity is immutable after creation
    TId Id { get; }
}

/// <summary>
/// Default entity interface using <see cref="Guid"/> as the identifier type.
/// </summary>
// Contract: pre=none, post=Id is Guid and auto-generated in Entity base class
public interface IEntity : IEntity<Guid>;