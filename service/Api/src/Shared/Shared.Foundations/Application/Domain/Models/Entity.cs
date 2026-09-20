using Shared.Application.Domain.Concerns.Entities;

namespace Shared.Application.Domain.Models;

/// <summary>
/// Base class for all domain entities providing common entity functionality.
/// Entities are equal if their identifiers are equal.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
// Boundary: Domain → Infrastructure — do not import EF Core or persistence concerns
// Contract: pre=TId is equatable, post=equality based on Id, not reference
public abstract class Entity<TId> : IEntity<TId>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    // AgentHint: do NOT expose setter publicly; use SetId() for explicit assignment
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Sets the identifier of the entity.
    /// </summary>
    /// <param name="id">The unique identifier to set.</param>
    protected void SetId(TId id)
    {
        // Assign: Explicitly set the entity's primary identifier
        Id = id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        // Check: Type verification for equality
        if (obj is not Entity<TId> other)
            return false;

        // Check: Reference equality for performance optimization
        if (ReferenceEquals(this, other))
            return true;

        // Guard: Handle uninitialized entities
        if (Id is null || other.Id is null)
            return false;

        // Check: Identifier-based equality (identity, not value)
        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Equality operator for entities.
    /// </summary>
    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Inequality operator for entities.
    /// </summary>
    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
    {
        return !(a == b);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Compute: Hash code derived from entity identity only
        return Id is null ? 0 : Id.GetHashCode() ^ 31;
    }
}

/// <summary>
/// Default entity base class using <see cref="Guid"/> as the identifier type.
/// Automatically generates a new GUID on initialization.
/// </summary>
// Contract: post=Id is auto-generated as new Guid on construction
public abstract class Entity : Entity<Guid>, IEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class with a new GUID.
    /// </summary>
    protected Entity()
    {
        // Initialize: Assign a new globally unique identifier on entity construction
        Id = Guid.NewGuid();
    }
}