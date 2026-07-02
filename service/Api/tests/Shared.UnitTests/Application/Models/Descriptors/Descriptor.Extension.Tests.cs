using Shared.Application.Models.Descriptors;


namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Descriptor")]
public sealed class DescriptorExtensionTests
{
    [Fact(DisplayName = "Format returns Name when only Name is set")]
    public void Format_WithOnlyName_ShouldReturnName()
    {
        Descriptor descriptor = new()
        {
            Name = "Products"
        };

        string result = descriptor.Format();

        result.Should().Be("Products");
    }

    [Fact(DisplayName = "Format returns Name: Description when both set")]
    public void Format_WithNameAndDescription_ShouldIncludeBoth()
    {
        Descriptor descriptor = new()
        {
            Name = "Read Products",
            Description = "Allows reading products."
        };

        string result = descriptor.Format();

        result.Should().Be("Read Products: Allows reading products.");
    }

    [Fact(DisplayName = "Format appends example when Example is set")]
    public void Format_WithExample_ShouldIncludeExample()
    {
        Descriptor descriptor = new()
        {
            Name = "Search",
            Example = "query=shirt"
        };

        string result = descriptor.Format();

        result.Should().Be("Search (e.g. query=shirt)");
    }

    [Fact(DisplayName = "Format includes Description and Example when all set")]
    public void Format_WithAllProperties_ShouldIncludeAll()
    {
        Descriptor descriptor = new()
        {
            Name = "Manage",
            Description = "Full access.",
            Example = "admin"
        };

        string result = descriptor.Format();

        result.Should().Be("Manage: Full access. (e.g. admin)");
    }

    [Fact(DisplayName = "WithName returns new Descriptor with updated Name")]
    public void WithName_ShouldReturnNewDescriptor()
    {
        Descriptor original = new()
        {
            Name = "Original",
            Description = "Test"
        };

        Descriptor updated = original.WithName("Updated");

        updated.Name.Should().Be("Updated");
        updated.Description.Should().Be("Test");
    }

    [Fact(DisplayName = "WithName preserves original descriptor")]
    public void WithName_ShouldPreserveOriginal()
    {
        Descriptor original = new()
        {
            Name = "Original"
        };

        _ = original.WithName("Updated");

        original.Name.Should().Be("Original");
    }

    [Fact(DisplayName = "WithDescription returns new Descriptor with updated Description")]
    public void WithDescription_ShouldReturnNewDescriptor()
    {
        Descriptor original = new()
        {
            Name = "Test",
            Description = "Old desc"
        };

        Descriptor updated = original.WithDescription("New desc");

        updated.Description.Should().Be("New desc");
        updated.Name.Should().Be("Test");
    }

    [Fact(DisplayName = "WithDescription accepts null to clear")]
    public void WithDescription_WithNull_ShouldClear()
    {
        Descriptor original = new()
        {
            Name = "Test",
            Description = "Old desc"
        };

        Descriptor updated = original.WithDescription(null);

        updated.Description.Should().BeNull();
    }

    [Fact(DisplayName = "WithExample returns new Descriptor with updated Example")]
    public void WithExample_ShouldReturnNewDescriptor()
    {
        Descriptor original = new()
        {
            Name = "Test",
            Example = "old"
        };

        Descriptor updated = original.WithExample("new");

        updated.Example.Should().Be("new");
    }

    [Fact(DisplayName = "WithExample accepts null to clear")]
    public void WithExample_WithNull_ShouldClear()
    {
        Descriptor original = new()
        {
            Name = "Test",
            Example = "old"
        };

        Descriptor updated = original.WithExample(null);

        updated.Example.Should().BeNull();
    }
}

