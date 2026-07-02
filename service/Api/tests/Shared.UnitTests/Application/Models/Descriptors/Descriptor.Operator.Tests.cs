using Shared.Application.Models.Descriptors;

namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Descriptor")]
public sealed class DescriptorOperatorTests
{
    [Fact(DisplayName = "Descriptor implicit operator string returns Name")]
    public void ImplicitOperatorString_ShouldReturnName()
    {
        Descriptor descriptor = new()
        {
            Name = "Test Item",
            Description = "A test."
        };

        string result = descriptor;

        result.Should().Be("Test Item");
    }

    [Fact(DisplayName = "Descriptor implicit operator string with null Description")]
    public void ImplicitOperatorString_WithNullDescription_ShouldReturnName()
    {
        Descriptor descriptor = new()
        {
            Name = "Minimal"
        };

        string result = descriptor;

        result.Should().Be("Minimal");
    }

    [Fact(DisplayName = "Descriptor implicit operator string can be passed to string parameter")]
    public void ImplicitOperatorString_ShouldPassToStringParameter()
    {
        Descriptor descriptor = new()
        {
            Name = "Parameter"
        };

        static void AcceptString(string value) { }

        Action act = () => AcceptString(descriptor);

        act.Should().NotThrow();
    }
}
