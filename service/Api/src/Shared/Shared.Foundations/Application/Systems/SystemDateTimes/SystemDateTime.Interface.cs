namespace Shared.Application.Systems.SystemDateTimes;

/// <summary>
/// Provides an abstraction for accessing the current system time.
/// Allows for easier unit testing by mocking the time.
/// </summary>
public interface ISystemDateTime
{
    /// <summary>
    /// Gets the current system date and time in UTC as a DateTimeOffset.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets today's date in UTC.
    /// </summary>
    DateTimeOffset Today { get; }
}
