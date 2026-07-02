using System.Collections;

namespace Shared.Application.Models.Optionals;

#pragma warning disable CA1000
public readonly partial struct Optional<T>
{
    #region Factory
    // Create: Wrap non-null value in Some optional — preserves non-null contract
    /// <summary>Creates an optional that contains a value.</summary>
    public static Optional<T> Some(T value)
    {
        // Guard: Reject null — Optional requires non-null for Some
        ArgumentNullException.ThrowIfNull(value);
        return new Optional<T>(value, true);
    }

    // Convert: Implicitly lift value into Optional — enables T → Optional<T> assignment
    /// <summary>Implicitly converts a value to Some(value).</summary>
    public static implicit operator Optional<T>(T value)
    {
        if (value is null)
            return None;
        return Some(value);
    }

    // Convert: Extract value from Optional — throws if empty
    /// <summary>Explicit conversion from Optional<T> to T – throws if empty.</summary>
    public static explicit operator T(Optional<T> optional) => optional.Value;
    #endregion

    #region Functional Methods
    // Transform: Apply selector to contained value, or propagate None
    /// <summary>Maps the contained value using a selector, if present.</summary>
    public Optional<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        // Guard: Selector must not be null
        ArgumentNullException.ThrowIfNull(selector);
        return HasValue ? Optional<TResult>.Some(selector(_value)) : Optional<TResult>.None;
    }

    // Transform: Flat-map contained value through binder returning Optional — enables chaining
    /// <summary>Binds (flat‑maps) the contained value using a binder that returns an Optional.</summary>
    public Optional<TResult> Bind<TResult>(Func<T, Optional<TResult>> binder)
    {
        // Guard: Binder must not be null
        ArgumentNullException.ThrowIfNull(binder);
        return HasValue ? binder(_value) : Optional<TResult>.None;
    }

    // Filter: Return None if predicate fails, otherwise pass through unchanged
    /// <summary>Filters the optional; if the predicate fails, returns None.</summary>
    public Optional<T> Filter(Func<T, bool> predicate)
    {
        // Guard: Predicate must not be null
        ArgumentNullException.ThrowIfNull(predicate);
        return HasValue && predicate(_value) ? this : None;
    }
    #endregion

    #region Retrieval
    // Retrieve: Return contained value or static fallback when empty
    /// <summary>Returns the contained value, or the provided fallback if empty.</summary>
    public T OrElse(T fallback) => HasValue ? _value : fallback;

    // Retrieve: Return contained value or computed fallback when empty
    /// <summary>Returns the contained value, or the result of a fallback function.</summary>
    public T OrElseGet(Func<T> fallback)
    {
        // Guard: Fallback factory must not be null
        ArgumentNullException.ThrowIfNull(fallback);
        return HasValue ? _value : fallback();
    }

    // Retrieve: Return contained value or throw when empty — custom exception factory supported
    /// <summary>Throws if empty; otherwise returns the value.</summary>
    public T OrElseThrow(Func<Exception>? exceptionFactory = null)
    {
        if (HasValue) return _value;
        // Raise: Throw configured or default exception — caller must handle at boundary
        throw exceptionFactory?.Invoke() ?? new InvalidOperationException("Optional is empty.");
    }

    // Execute: Invoke action on contained value if present — fire-and-forget side effect
    /// <summary>Performs an action on the contained value, if present.</summary>
    public void IfPresent(Action<T> action)
    {
        // Guard: Action must not be null
        ArgumentNullException.ThrowIfNull(action);
        if (HasValue) action(_value);
    }
    #endregion

    #region LINQ Support
    // Enumerate: Yield contained value for foreach and LINQ query compatibility
    public IEnumerator<T> GetEnumerator()
    {
        if (HasValue) yield return _value;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Equality
    // Compare: Structural equality — same HasValue and matching values
    public bool Equals(Optional<T> other)
        => HasValue == other.HasValue && (!HasValue || EqualityComparer<T>.Default.Equals(_value, other._value));

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    // Compute: Hash code from HasValue and contained value — consistent with Equals
    public override int GetHashCode()
        => HasValue ? HashCode.Combine(HasValue, _value) : HashCode.Combine(HasValue);

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);
    public static bool operator !=(Optional<T> left, Optional<T> right) => !(left == right);
    #endregion

    #region ToString
    // Format: Display as "Some(value)" or "None" for debugging and logging
    public override string ToString() => HasValue ? $"Some({_value})" : "None";
    #endregion
}
#pragma warning restore CA1000
