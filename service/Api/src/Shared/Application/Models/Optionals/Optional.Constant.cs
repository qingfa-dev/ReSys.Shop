namespace Shared.Application.Models.Optionals;

#pragma warning disable CA1000
public readonly partial struct Optional<T>
{
    #region Constants
    // Create: Empty sentinel for optional without value — consumers check HasValue before access
    /// <summary>Creates an empty optional.</summary>
    public static Optional<T> None => new Optional<T>(default!, false);
    #endregion
}
#pragma warning restore CA1000
