using Shared.Operational.Persistence.Configurations.Enums;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Enums;

public enum TestColor
{
    Red = 0,
    Green = 1,
    Blue = 2
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class CustomEnumToStringConverterTests
{
    public class ConvertToProvider
    {
        [Fact(DisplayName = "Should convert enum value to string")]
        public void ShouldConvertEnumToString()
        {
            CustomEnumToStringConverter<TestColor> converter = new();

            String result = (String)converter.ConvertToProvider(TestColor.Green)!;

            result.Should().Be("Green");
        }

        [Fact(DisplayName = "Should convert enum value with default ToString")]
        public void ShouldConvertEnumWithDefaultToString()
        {
            CustomEnumToStringConverter<TestColor> converter = new();

            String result = (String)converter.ConvertToProvider(TestColor.Blue)!;

            result.Should().Be("Blue");
        }
    }

    public class ConvertFromProvider
    {
        [Fact(DisplayName = "Should parse string to enum value")]
        public void ShouldParseStringToEnum()
        {
            CustomEnumToStringConverter<TestColor> converter = new();

            TestColor result = (TestColor)converter.ConvertFromProvider("Green")!;

            result.Should().Be(TestColor.Green);
        }

        [Fact(DisplayName = "Should parse string case-insensitively")]
        public void ShouldParseCaseInsensitively()
        {
            CustomEnumToStringConverter<TestColor> converter = new();

            TestColor result = (TestColor)converter.ConvertFromProvider("blue")!;

            result.Should().Be(TestColor.Blue);
        }

        [Fact(DisplayName = "Should parse uppercase string")]
        public void ShouldParseUppercase()
        {
            CustomEnumToStringConverter<TestColor> converter = new();

            TestColor result = (TestColor)converter.ConvertFromProvider("RED")!;

            result.Should().Be(TestColor.Red);
        }
    }

    public class RoundTrip
    {
        [Fact(DisplayName = "Should round-trip enum value through convert and back")]
        public void ShouldRoundTrip()
        {
            CustomEnumToStringConverter<TestColor> converter = new();

            String intermediate = (String)converter.ConvertToProvider(TestColor.Green)!;
            TestColor result = (TestColor)converter.ConvertFromProvider(intermediate)!;

            result.Should().Be(TestColor.Green);
        }
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class CustomNullableEnumToStringConverterTests
{
    public class ConvertToProvider
    {
        [Fact(DisplayName = "Should convert enum value to string")]
        public void ShouldConvertEnumToString()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();
            TestColor? value = TestColor.Green;

            String? result = (String?)converter.ConvertToProvider(value);

            result.Should().Be("Green");
        }

        [Fact(DisplayName = "Should return null when value is null")]
        public void ShouldReturnNullForNullInput()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();

            String? result = (String?)converter.ConvertToProvider(null);

            result.Should().BeNull();
        }
    }

    public class ConvertFromProvider
    {
        [Fact(DisplayName = "Should parse string to enum value")]
        public void ShouldParseStringToEnum()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();

            TestColor? result = (TestColor?)converter.ConvertFromProvider("Green");

            result.Should().Be(TestColor.Green);
        }

        [Fact(DisplayName = "Should return null for null input")]
        public void ShouldReturnNullForNullInput()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();

            TestColor? result = (TestColor?)converter.ConvertFromProvider(null);

            result.Should().BeNull();
        }

        [Fact(DisplayName = "Should return null for empty string input")]
        public void ShouldReturnNullForEmptyString()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();

            TestColor? result = (TestColor?)converter.ConvertFromProvider(String.Empty);

            result.Should().BeNull();
        }
    }

    public class RoundTrip
    {
        [Fact(DisplayName = "Should round-trip enum value")]
        public void ShouldRoundTrip()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();
            TestColor? original = TestColor.Blue;

            String? intermediate = (String?)converter.ConvertToProvider(original);
            TestColor? result = (TestColor?)converter.ConvertFromProvider(intermediate);

            result.Should().Be(original);
        }

        [Fact(DisplayName = "Should round-trip null value")]
        public void ShouldRoundTripNull()
        {
            CustomNullableEnumToStringConverter<TestColor> converter = new();

            String? intermediate = (String?)converter.ConvertToProvider(null);
            TestColor? result = (TestColor?)converter.ConvertFromProvider(intermediate);

            result.Should().BeNull();
        }
    }
}
