using Shared.Application.Models.Descriptors;

namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OptionDescriptor")]
public sealed class OptionDescriptorOperatorTests
{
    [Fact(DisplayName = "OptionDescriptor implicit operator TValue returns Value")]
    public void ImplicitOperator_ShouldReturnValue()
    {
        OptionDescriptor<int> option = new()
        {
            Value = 42,
            Name = "The Answer"
        };

        int result = option;

        result.Should().Be(42);
    }

    [Fact(DisplayName = "OptionDescriptor implicit operator string returns Value")]
    public void ImplicitOperator_WithString_ShouldReturnValue()
    {
        OptionDescriptor<string> option = new()
        {
            Value = "admin.catalog.products.read",
            Name = "Read Products"
        };

        string result = option;

        result.Should().Be("admin.catalog.products.read");
    }

    [Fact(DisplayName = "OptionDescriptor implicit operator can be used as integer")]
    public void ImplicitOperator_ShouldWorkAsIntParameter()
    {
        OptionDescriptor<int> option = new()
        {
            Value = 100,
            Name = "Max Items"
        };

        static void AcceptInt(int value) { }

        Action act = () => AcceptInt(option);

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "OptionDescriptor implicit operator preserves value across conversions")]
    public void ImplicitOperator_WithComplexType_ShouldPreserveValue()
    {
        Guid expected = Guid.NewGuid();
        OptionDescriptor<Guid> option = new()
        {
            Value = expected,
            Name = "Identifier"
        };

        Guid result = option;

        result.Should().Be(expected);
    }
}
