using Shared.Application.Models.Descriptors;

namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Descriptor")]
public sealed class DescriptorMethodTests
{
    [Fact(DisplayName = "Named factory creates descriptor with required Name")]
    public void Named_ShouldCreateWithName()
    {
        Descriptor descriptor = Descriptor.Named("products");

        descriptor.Name.Should().Be("products");
        descriptor.Description.Should().BeNull();
        descriptor.Example.Should().BeNull();
    }

    [Fact(DisplayName = "Named factory creates descriptor with Description")]
    public void Named_WithDescription_ShouldSetDescription()
    {
        Descriptor descriptor = Descriptor.Named(
            "products",
            "Product catalog resource.");

        descriptor.Name.Should().Be("products");
        descriptor.Description.Should().Be("Product catalog resource.");
        descriptor.Example.Should().BeNull();
    }

    [Fact(DisplayName = "Named factory creates descriptor with Example")]
    public void Named_WithExample_ShouldSetExample()
    {
        Descriptor descriptor = Descriptor.Named(
            "products",
            "Product catalog.",
            new { count = 100 });

        descriptor.Name.Should().Be("products");
        descriptor.Description.Should().Be("Product catalog.");
        descriptor.Example.Should().BeEquivalentTo(new { count = 100 });
    }

    [Fact(DisplayName = "Named factory creates descriptor with all parameters")]
    public void Named_WithAllParameters_ShouldSetAll()
    {
        Descriptor descriptor = Descriptor.Named(
            "admin.catalog.products.read",
            "Allows reading products.");

        descriptor.Name.Should().Be("admin.catalog.products.read");
        descriptor.Description.Should().Be("Allows reading products.");
    }

    [Fact(DisplayName = "Multiple Named calls create distinct instances")]
    public void Named_MultipleCalls_ShouldCreateDistinct()
    {
        Descriptor first = Descriptor.Named("products");
        Descriptor second = Descriptor.Named("orders");

        first.Name.Should().Be("products");
        second.Name.Should().Be("orders");
    }
}
