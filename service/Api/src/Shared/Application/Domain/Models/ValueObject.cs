namespace Shared.Application.Domain.Models;

/// <summary>
/// Base class for DDD value objects.
/// Value objects are equal if all their components match.
/// They do not have a unique identifier and are immutable.
/// </summary>
// Invariant: Equality depends entirely on GetEqualityComponents(); all properties defining value identity must be included there
// Contract: post=equality based on GetEqualityComponents(), not reference
public abstract class ValueObject
{
    /// <summary>
    /// Gets the components that define the identity of the value object.
    /// All properties that contribute to the value object's state should be returned here.
    /// </summary>
    /// <returns>An enumeration of objects representing the state components.</returns>
    // AgentHint: include ALL properties that define value identity; ignore computed fields
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        // Check: Null and type verification for structural equality
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        // Compute: Structural equality from GetEqualityComponents() comparison
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Compute: Hash code aggregated from all equality components
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(1, (current, next) => HashCode.Combine(current, next));
    }

    /// <summary>
    /// Equality operator for value objects.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for value objects.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
