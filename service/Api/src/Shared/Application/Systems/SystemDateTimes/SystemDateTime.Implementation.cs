namespace Shared.Application.Systems.SystemDateTimes;

/// <summary>Provides the current system time via DateTimeOffset.UtcNow — injectable abstraction for testability.</summary>
// Invariant: UtcNow always returns UTC; Today returns DateTimeOffset with time component at 00:00:00 local.
public sealed class SystemDateTime : ISystemDateTime
{
    /// <summary>Returns the current UTC system time with offset.</summary>
    // Contract: post=return.Kind==DateTimeKind.Utc
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <summary>Returns the current system date at midnight local time.</summary>
    // Contract: post=return.TimeOfDay==TimeSpan.Zero
    public DateTimeOffset Today => DateTime.Today;
}
