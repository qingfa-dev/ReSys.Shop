using Shared.Application.Models.Descriptors;

namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OptionDescriptor")]
public sealed class OptionDescriptorMethodTests
{
    [Fact(DisplayName = "Option factory creates descriptor with required Value and Name")]
    public void Option_ShouldCreateWithValueAndName()
    {
        OptionDescriptor<string> option = OptionDescriptor<string>.Option(
            "admin.catalog.products.read",
            "Read Products");

        option.Value.Should().Be("admin.catalog.products.read");
        option.Name.Should().Be("Read Products");
        option.Description.Should().BeNull();
        option.Example.Should().BeNull();
    }

    [Fact(DisplayName = "Option factory creates descriptor with Description")]
    public void Option_WithDescription_ShouldSetDescription()
    {
        OptionDescriptor<string> option = OptionDescriptor<string>.Option(
            "admin.catalog.products.manage",
            "Manage Products",
            "Allows managing all products.");

        option.Value.Should().Be("admin.catalog.products.manage");
        option.Name.Should().Be("Manage Products");
        option.Description.Should().Be("Allows managing all products.");
    }

    [Fact(DisplayName = "Option factory creates descriptor with Example")]
    public void Option_WithExample_ShouldSetExample()
    {
        OptionDescriptor<string> option = OptionDescriptor<string>.Option(
            "store.search.products.search",
            "Search Products",
            "Allows searching.",
            "query=shirt");

        option.Value.Should().Be("store.search.products.search");
        option.Name.Should().Be("Search Products");
        option.Description.Should().Be("Allows searching.");
        option.Example.Should().Be("query=shirt");
    }

    [Fact(DisplayName = "Option factory works with integer TValue")]
    public void Option_WithIntValue_ShouldCreateDescriptor()
    {
        OptionDescriptor<int> option = OptionDescriptor<int>.Option(
            42,
            "The Answer",
            "Ultimate answer.");

        option.Value.Should().Be(42);
        option.Name.Should().Be("The Answer");
        option.Description.Should().Be("Ultimate answer.");
    }

    [Fact(DisplayName = "Option factory works with enum TValue")]
    public void Option_WithEnumValue_ShouldCreateDescriptor()
    {
        OptionDescriptor<SamplePermission> option = OptionDescriptor<SamplePermission>.Option(
            SamplePermission.Read,
            "Read Permission");

        option.Value.Should().Be(SamplePermission.Read);
        option.Name.Should().Be("Read Permission");
    }

    [Fact(DisplayName = "Multiple Option calls create distinct instances")]
    public void Option_MultipleCalls_ShouldCreateDistinctInstances()
    {
        OptionDescriptor<string> first = OptionDescriptor<string>.Option(
            "store.catalog.products.read",
            "Read Products");

        OptionDescriptor<string> second = OptionDescriptor<string>.Option(
            "store.catalog.products.manage",
            "Manage Products");

        first.Value.Should().Be("store.catalog.products.read");
        second.Value.Should().Be("store.catalog.products.manage");
    }

    private enum SamplePermission
    {
        Read,
        Write,
        Admin
    }
}
