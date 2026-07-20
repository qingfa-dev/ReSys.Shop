using Shared.Governance.OpenApi.Options;

namespace Shared.UnitTests.Governance.OpenApi;

// Test fixture types — at namespace level (not nested in test class) so that
// GetSchemaReferenceId produces clean expected names without the test class prefix.

internal sealed class NamingSimple { }

internal sealed class NamingOuter
{
    internal sealed class Inner { }

    internal struct NestedValue { }

    internal sealed class Deeply
    {
        internal sealed class Nested { }
    }
}

internal sealed class Wrapper<T> { }

internal sealed class Dual<T1, T2> { }

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OpenApi")]
public sealed class OpenApiSchemaNamingTests
{
    public static TheoryData<Type, string> NamingCases
    {
        get
        {
            TheoryData<Type, string> data = new();

            // Simple non-nested, non-generic
            data.Add(typeof(NamingSimple), "NamingSimple");

            // Single-level nested
            data.Add(typeof(NamingOuter.Inner), "NamingOuterInner");

            // Deeply nested (2+ levels)
            data.Add(typeof(NamingOuter.Deeply.Nested), "NamingOuterDeeplyNested");

            // Generic with single nested type argument
            data.Add(typeof(Wrapper<NamingOuter.Inner>), "WrapperOfNamingOuterInner");

            // Generic with multiple different nested type arguments
            data.Add(typeof(Dual<NamingOuter.Inner, NamingOuter.Deeply.Nested>), "DualOfNamingOuterInnerAndNamingOuterDeeplyNested");

            // Generic with repeated same nested type argument
            data.Add(typeof(Dual<NamingOuter.Inner, NamingOuter.Inner>), "DualOfNamingOuterInnerAndNamingOuterInner");

            // Generic wrapper wrapping a generic wrapper containing a nested type
            data.Add(typeof(Wrapper<Wrapper<NamingOuter.Inner>>), "WrapperOfWrapperOfNamingOuterInner");

            return data;
        }
    }

    [Theory(DisplayName = "GetSchemaReferenceId should produce expected schema name")]
    [MemberData(nameof(NamingCases))]
    public void GetSchemaReferenceId_ShouldProduceExpectedName(Type inputType, string expectedName)
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(inputType);

        result.Should().Be(expectedName);
    }

    [Fact(DisplayName = "GetSchemaReferenceId should unwrap Nullable<T> for nested value type")]
    public void GetSchemaReferenceId_ShouldUnwrapNullable_WhenNestedValueType()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(Nullable<NamingOuter.NestedValue>));

        result.Should().Be("NamingOuterNestedValue");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should prefix 'ArrayOf' for simple array")]
    public void GetSchemaReferenceId_ShouldPrefixArrayOf_WhenSimpleArray()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(int[]));

        result.Should().Be("ArrayOfInt32");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should prefix 'ArrayOf' for array of nested type")]
    public void GetSchemaReferenceId_ShouldPrefixArrayOf_WhenNestedArray()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(NamingOuter.NestedValue[]));

        result.Should().Be("ArrayOfNamingOuterNestedValue");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should handle generic with array of nested type argument")]
    public void GetSchemaReferenceId_ShouldHandleGenericWithNestedArrayArg()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(Wrapper<NamingOuter.NestedValue[]>));

        result.Should().Be("WrapperOfArrayOfNamingOuterNestedValue");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should produce 'AnonymousTypeOf' for anonymous types")]
    public void GetSchemaReferenceId_ShouldHandleAnonymousType()
    {
        object anon = new { X = 1, Y = "hello" };
        Type anonType = anon.GetType();

        string result = OpenApiSchemaNaming.GetSchemaReferenceId(anonType);

        result.Should().Be("AnonymousTypeOfInt32AndString");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should return 'Int32' for primitive int")]
    public void GetSchemaReferenceId_ShouldReturnInt32_WhenPrimitiveInt()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(int));

        result.Should().Be("Int32");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should return 'String' for string type")]
    public void GetSchemaReferenceId_ShouldReturnString_WhenStringType()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(string));

        result.Should().Be("String");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should return 'Boolean' for bool")]
    public void GetSchemaReferenceId_ShouldReturnBoolean_WhenBool()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(bool));

        result.Should().Be("Boolean");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should handle open generic type definition")]
    public void GetSchemaReferenceId_ShouldHandleOpenGeneric()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(Wrapper<>));

        result.Should().Be("WrapperOfT");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should return 'Decimal' for decimal")]
    public void GetSchemaReferenceId_ShouldReturnDecimal_WhenDecimal()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(decimal));

        result.Should().Be("Decimal");
    }

    [Fact(DisplayName = "GetSchemaReferenceId should return 'Int64' for long")]
    public void GetSchemaReferenceId_ShouldReturnInt64_WhenLong()
    {
        string result = OpenApiSchemaNaming.GetSchemaReferenceId(typeof(long));

        result.Should().Be("Int64");
    }
}
