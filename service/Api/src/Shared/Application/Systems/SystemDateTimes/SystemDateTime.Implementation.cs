namespace Shared.Application.Systems.SystemDateTimes;

/// <summary>
/// Standard implementation of <see cref="ISystemDateTime"/> using system clock.
/// </summary>
public sealed class SystemDateTime : ISystemDateTime
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; // Receive: Current system time offset in UTC

    /// <inheritdoc />
    public DateTimeOffset Today => DateTime.Today; // Receive: Current system date
}
