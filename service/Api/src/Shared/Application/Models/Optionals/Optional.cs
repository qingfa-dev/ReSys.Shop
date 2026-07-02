namespace Shared.Application.Models.Optionals;

/// <summary>
/// Represents an optional value that can be either Some(value) or None.
/// </summary>
/// <typeparam name="T">The type of the contained value.</typeparam>
public readonly partial struct Optional<T> : IEquatable<Optional<T>>, IEnumerable<T>
{
    #region Fields
    private readonly T _value;
    #endregion

    #region Constructors
    // Create: Initialize Optional<T> with value and presence flag
    internal Optional(T value, bool hasValue)
    {
        _value = value;
        HasValue = hasValue;
    }
    #endregion

    #region Properties
    /// <summary>True if the optional contains a value.</summary>
    public bool HasValue { get; }

    /// <summary>True if the optional is empty.</summary>
    public bool IsNone => !HasValue;

    /// <summary>Gets the contained value. Throws if none.</summary>
    /// <exception cref="InvalidOperationException">When the optional is empty.</exception>
    // Guard: Prevent access to value when optional is empty — caller must check HasValue first
    public T Value => HasValue
        ? _value
        : throw new InvalidOperationException("Optional has no value.");
    #endregion
}
