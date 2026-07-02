namespace Shared.Application.Models.Optionals;

#pragma warning disable CA1000
public readonly partial struct Optional<T>
{
    #region Implicit Operators
    // Convert: Allow direct boolean check — None is falsy, Some is truthy
    //          Enables idiomatic C# patterns: if (optional) { ... }
    /// <summary>Allows direct boolean check: if (optional) ...</summary>
    public static implicit operator bool(Optional<T> optional) => optional.HasValue;
    #endregion
}
#pragma warning restore CA1000
