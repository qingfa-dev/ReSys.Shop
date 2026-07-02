using Shared.Operational.Persistence.Configurations.DateTimes;

namespace Shared.UnitTests.Operational.Persistence.Configurations.DateTimes;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class UtcDateTimeOffsetValueConverterTests
{
    public class ConvertToProvider
    {
        [Fact(DisplayName = "Should convert DateTimeOffset to ISO 8601 string")]
        public void ShouldConvertToIso8601String()
        {
            UtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset value = new(2025, 4, 25, 10, 30, 0, TimeSpan.Zero);

            string result = (string)converter.ConvertToProvider(value)!;

            result.Should().Be("2025-04-25T10:30:00.0000000+00:00");
        }

        [Fact(DisplayName = "Should convert non-UTC DateTimeOffset with offset preserved")]
        public void ShouldPreserveOffset()
        {
            UtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset value = new(2025, 4, 25, 10, 30, 0, TimeSpan.FromHours(2));

            string result = (string)converter.ConvertToProvider(value)!;

            result.Should().Be("2025-04-25T10:30:00.0000000+02:00");
        }
    }

    public class ConvertFromProvider
    {
        [Fact(DisplayName = "Should parse ISO 8601 string to DateTimeOffset")]
        public void ShouldParseIso8601String()
        {
            UtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset result = (DateTimeOffset)converter.ConvertFromProvider("2025-04-25T10:30:00.0000000+00:00")!;

            result.Should().Be(new DateTimeOffset(2025, 4, 25, 10, 30, 0, TimeSpan.Zero));
        }

        [Fact(DisplayName = "Should parse ISO 8601 string with non-zero offset")]
        public void ShouldParseWithNonZeroOffset()
        {
            UtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset result = (DateTimeOffset)converter.ConvertFromProvider("2025-04-25T10:30:00.0000000+02:00")!;

            result.Should().Be(new DateTimeOffset(2025, 4, 25, 10, 30, 0, TimeSpan.FromHours(2)));
        }
    }

    public class RoundTrip
    {
        [Fact(DisplayName = "Should round-trip DateTimeOffset through convert and back")]
        public void ShouldRoundTrip()
        {
            UtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset original = new(2026, 6, 19, 15, 45, 30, 123, TimeSpan.Zero);

            string intermediate = (string)converter.ConvertToProvider(original)!;
            DateTimeOffset result = (DateTimeOffset)converter.ConvertFromProvider(intermediate)!;

            result.Should().Be(original);
        }

        [Fact(DisplayName = "Should round-trip with non-zero offset")]
        public void ShouldRoundTripWithNonZeroOffset()
        {
            UtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset original = new(2026, 6, 19, 15, 45, 30, TimeSpan.FromHours(-5));

            string intermediate = (string)converter.ConvertToProvider(original)!;
            DateTimeOffset result = (DateTimeOffset)converter.ConvertFromProvider(intermediate)!;

            result.Should().Be(original);
        }
    }

    public class MinMaxValues
    {
        [Fact(DisplayName = "Should round-trip DateTimeOffset.MinValue")]
        public void ShouldHandleMinValue()
        {
            UtcDateTimeOffsetValueConverter converter = new();

            string intermediate = (string)converter.ConvertToProvider(DateTimeOffset.MinValue)!;
            DateTimeOffset result = (DateTimeOffset)converter.ConvertFromProvider(intermediate)!;

            result.Should().Be(DateTimeOffset.MinValue);
        }

        [Fact(DisplayName = "Should round-trip DateTimeOffset.MaxValue")]
        public void ShouldHandleMaxValue()
        {
            UtcDateTimeOffsetValueConverter converter = new();

            string intermediate = (string)converter.ConvertToProvider(DateTimeOffset.MaxValue)!;
            DateTimeOffset result = (DateTimeOffset)converter.ConvertFromProvider(intermediate)!;

            result.Should().Be(DateTimeOffset.MaxValue);
        }
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class NullableUtcDateTimeOffsetValueConverterTests
{
    public class ConvertToProvider
    {
        [Fact(DisplayName = "Should convert DateTimeOffset to ISO 8601 string")]
        public void ShouldConvertToIso8601String()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset value = new(2025, 4, 25, 10, 30, 0, TimeSpan.Zero);

            string result = (string)converter.ConvertToProvider(value)!;

            result.Should().Be("2025-04-25T10:30:00.0000000+00:00");
        }

        [Fact(DisplayName = "Should return null when value is null")]
        public void ShouldReturnNullForNullInput()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();

            string? result = (string?)converter.ConvertToProvider(null);

            result.Should().BeNull();
        }
    }

    public class ConvertFromProvider
    {
        [Fact(DisplayName = "Should parse ISO 8601 string to DateTimeOffset")]
        public void ShouldParseIso8601String()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset? result = (DateTimeOffset?)converter.ConvertFromProvider("2025-04-25T10:30:00.0000000+00:00");

            result.Should().Be(new DateTimeOffset(2025, 4, 25, 10, 30, 0, TimeSpan.Zero));
        }

        [Fact(DisplayName = "Should return null for null input")]
        public void ShouldReturnNullForNullInput()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();

            DateTimeOffset? result = (DateTimeOffset?)converter.ConvertFromProvider(null);

            result.Should().BeNull();
        }

        [Fact(DisplayName = "Should return null for empty string input")]
        public void ShouldReturnNullForEmptyString()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();

            DateTimeOffset? result = (DateTimeOffset?)converter.ConvertFromProvider(string.Empty);

            result.Should().BeNull();
        }
    }

    public class RoundTrip
    {
        [Fact(DisplayName = "Should round-trip nullable DateTimeOffset")]
        public void ShouldRoundTrip()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();
            DateTimeOffset original = DateTimeOffset.UtcNow;

            string? intermediate = (string?)converter.ConvertToProvider(original);
            DateTimeOffset? result = (DateTimeOffset?)converter.ConvertFromProvider(intermediate);

            result.Should().Be(original);
        }

        [Fact(DisplayName = "Should round-trip null value")]
        public void ShouldRoundTripNull()
        {
            NullableUtcDateTimeOffsetValueConverter converter = new();

            string? intermediate = (string?)converter.ConvertToProvider(null);
            DateTimeOffset? result = (DateTimeOffset?)converter.ConvertFromProvider(intermediate);

            result.Should().BeNull();
        }
    }
}
