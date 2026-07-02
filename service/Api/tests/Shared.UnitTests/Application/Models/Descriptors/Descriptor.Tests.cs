using Shared.Application.Models.Descriptors;

namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Descriptor")]
public sealed class DescriptorTests
{
    [Fact(DisplayName = "Descriptor struct sets Name, Description, Example")]
    public void Descriptor_ShouldSetAllProperties()
    {
        var example = new { threshold = 10 };

        Descriptor descriptor = new()
        {
            Name = "Test Item",
            Description = "A test descriptor.",
            Example = example
        };

        descriptor.Name.Should().Be("Test Item");
        descriptor.Description.Should().Be("A test descriptor.");
        descriptor.Example.Should().BeEquivalentTo(example);
    }

    [Fact(DisplayName = "Descriptor with only required Name")]
    public void Descriptor_WithOnlyName_ShouldSetDefaults()
    {
        Descriptor descriptor = new()
        {
            Name = "Test Item"
        };

        descriptor.Name.Should().Be("Test Item");
        descriptor.Description.Should().BeNull();
        descriptor.Example.Should().BeNull();
    }

    [Fact(DisplayName = "Descriptor implements IDescriptor")]
    public void Descriptor_ShouldImplementIDescriptor()
    {
        Descriptor descriptor = new()
        {
            Name = "Test Item",
            Description = "desc",
            Example = "example"
        };

#pragma warning disable CA1859 // Use concrete type — intentionally testing interface
        IDescriptor iface = descriptor;
#pragma warning restore CA1859

        iface.Name.Should().Be("Test Item");
        iface.Description.Should().Be("desc");
        iface.Example.Should().Be("example");
    }

    [Fact(DisplayName = "Descriptor value equality: same values are equal")]
    public void Descriptor_SameValues_ShouldBeEqual()
    {
        Descriptor a = new() { Name = "A", Description = "desc" };
        Descriptor b = new() { Name = "A", Description = "desc" };

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Descriptor value equality: different values are not equal")]
    public void Descriptor_DifferentValues_ShouldNotBeEqual()
    {
        Descriptor a = new() { Name = "A", Description = "desc" };
        Descriptor b = new() { Name = "B", Description = "desc" };

        a.Should().NotBe(b);
    }
}
