using System.Globalization;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Operational.Persistence.Configurations.DateTimes;

/// <summary>
/// Converts DateTimeOffset values to and from UTC ISO 8601 strings for database storage.
/// </summary>
public class UtcDateTimeOffsetValueConverter()
    : ValueConverter<DateTimeOffset, string>(
        // Transform: Convert DateTimeOffset to ISO 8601 string in UTC
        v => v.ToString("O", CultureInfo.InvariantCulture),
        // Transform: Parse ISO 8601 string back to UTC DateTimeOffset
        v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal));

/// <summary>
/// Converts nullable DateTimeOffset values to and from UTC ISO 8601 strings for database storage.
/// </summary>
public class NullableUtcDateTimeOffsetValueConverter()
    : ValueConverter<DateTimeOffset?, string?>(
        // Transform: Convert nullable DateTimeOffset to ISO 8601 string
        v => v == null ? null : v.Value.ToString("O", CultureInfo.InvariantCulture),
        // Transform: Parse nullable ISO 8601 string back to DateTimeOffset
        v => string.IsNullOrEmpty(v)
            ? null
            : DateTimeOffset.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal));
