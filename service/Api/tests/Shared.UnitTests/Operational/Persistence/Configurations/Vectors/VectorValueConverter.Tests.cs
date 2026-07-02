using System.Text.Json;

using Pgvector;

using Shared.Operational.Persistence.Configurations.Vectors;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Vectors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class VectorValueConverterTests
{
    public class ConvertToProvider
    {
        [Fact(DisplayName = "Should convert Vector to JSON string")]
        public void ShouldConvertVectorToJsonString()
        {
            VectorValueConverter converter = new();
            Vector value = new(new Single[] { 0.1f, 0.2f, 0.3f });

            String result = (String)converter.ConvertToProvider(value)!;

            result.Should().Be("[0.1,0.2,0.3]");
        }

        [Fact(DisplayName = "Should convert single-element Vector")]
        public void ShouldConvertSingleElementVector()
        {
            VectorValueConverter converter = new();
            Vector value = new(new float[] { 42.0f });

            String result = (String)converter.ConvertToProvider(value)!;

            result.Should().Be("[42]");
        }

        [Fact(DisplayName = "Should convert empty Vector")]
        public void ShouldConvertEmptyVector()
        {
            VectorValueConverter converter = new();
            Vector value = new(Array.Empty<Single>());

            String result = (String)converter.ConvertToProvider(value)!;

            result.Should().Be("[]");
        }
    }

    public class ConvertFromProvider
    {
        private static readonly float[] expectation = new Single[] { 0.1f, 0.2f, 0.3f };

        [Fact(DisplayName = "Should parse JSON string to Vector")]
        public void ShouldParseJsonStringToVector()
        {
            VectorValueConverter converter = new();

            Vector? result = (Vector?)converter.ConvertFromProvider("[0.1,0.2,0.3]");

            result.Should().NotBeNull();
            result!.ToArray().Should().BeEquivalentTo(expectation);
        }

        [Fact(DisplayName = "Should parse empty JSON array")]
        public void ShouldParseEmptyJsonArray()
        {
            VectorValueConverter converter = new();

            Vector? result = (Vector?)converter.ConvertFromProvider("[]");

            result.Should().NotBeNull();
            result!.ToArray().Should().BeEmpty();
        }

        [Fact(DisplayName = "Should throw on malformed JSON")]
        public void ShouldThrowOnMalformedJson()
        {
            VectorValueConverter converter = new();

            Action act = () => converter.ConvertFromProvider("not-json");

            act.Should().Throw<JsonException>();
        }
    }

    public class RoundTrip
    {
        [Fact(DisplayName = "Should round-trip Vector through convert and back")]
        public void ShouldRoundTrip()
        {
            VectorValueConverter converter = new();
            Vector original = new(new Single[] { 0.5f, 0.25f, 0.125f, 0.0625f });

            String intermediate = (String)converter.ConvertToProvider(original)!;
            Vector? result = (Vector?)converter.ConvertFromProvider(intermediate);

            result.Should().NotBeNull();
            result!.ToArray().Should().BeEquivalentTo(original.ToArray());
        }
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class NullableVectorValueConverterTests
{
    public class ConvertToProvider
    {
        [Fact(DisplayName = "Should convert Vector to JSON string")]
        public void ShouldConvertVectorToJsonString()
        {
            NullableVectorValueConverter converter = new();
            Vector? value = new(new Single[] { 0.1f, 0.2f, 0.3f });

            String? result = (String?)converter.ConvertToProvider(value);

            result.Should().Be("[0.1,0.2,0.3]");
        }

        [Fact(DisplayName = "Should return null for null input")]
        public void ShouldReturnNullForNullInput()
        {
            NullableVectorValueConverter converter = new();

            String? result = (String?)converter.ConvertToProvider(null);

            result.Should().BeNull();
        }
    }

    public class ConvertFromProvider
    {
        [Fact(DisplayName = "Should parse JSON string to Vector")]
        public void ShouldParseJsonStringToVector()
        {
            NullableVectorValueConverter converter = new();

            Vector? result = (Vector?)converter.ConvertFromProvider("[0.1,0.2,0.3]");

            result.Should().NotBeNull();
            result!.ToArray().Should().BeEquivalentTo(new float[] { 0.1f, 0.2f, 0.3f });
        }

        [Fact(DisplayName = "Should return null for null input")]
        public void ShouldReturnNullForNullInput()
        {
            NullableVectorValueConverter converter = new();

            Vector? result = (Vector?)converter.ConvertFromProvider(null);

            result.Should().BeNull();
        }

        [Fact(DisplayName = "Should return null for empty string input")]
        public void ShouldReturnNullForEmptyString()
        {
            NullableVectorValueConverter converter = new();

            Vector? result = (Vector?)converter.ConvertFromProvider(String.Empty);

            result.Should().BeNull();
        }
    }

    public class RoundTrip
    {
        [Fact(DisplayName = "Should round-trip Vector value")]
        public void ShouldRoundTrip()
        {
            NullableVectorValueConverter converter = new();
            Vector? original = new(new Single[] { 0.5f, 0.25f, 0.125f });

            String? intermediate = (String?)converter.ConvertToProvider(original);
            Vector? result = (Vector?)converter.ConvertFromProvider(intermediate);

            result.Should().NotBeNull();
            result!.ToArray().Should().BeEquivalentTo(original!.ToArray());
        }

        [Fact(DisplayName = "Should round-trip null value")]
        public void ShouldRoundTripNull()
        {
            NullableVectorValueConverter converter = new();

            String? intermediate = (String?)converter.ConvertToProvider(null);
            Vector? result = (Vector?)converter.ConvertFromProvider(intermediate);

            result.Should().BeNull();
        }
    }
}
