using Shared.Application.Domain.Concerns.Sluggable;

namespace Shared.UnitTests.Application.Domain.Concerns.Sluggable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class SluggableBehaviorTests
{
    private sealed class TestSluggable : ISluggable
    {
        public string Slug { get; set; } = string.Empty;
    }

    [Fact(DisplayName = "ApplySlugging should generate and assign slug")]
    public void ApplySlugging_ShouldGenerateAndAssignSlug()
    {
        var entity = new TestSluggable();
        var source = "Test Product Name";

        SluggableBehavior.ApplySlugging(entity, source);

        entity.Slug.Should().Be("test-product-name");
    }

    [Fact(DisplayName = "ApplySlugging should do nothing if source is empty")]
    public void ApplySlugging_ShouldDoNothing_WhenSourceIsEmpty()
    {
        var entity = new TestSluggable { Slug = "original-slug" };

        SluggableBehavior.ApplySlugging(entity, "");

        entity.Slug.Should().Be("original-slug");
    }

    [Theory(DisplayName = "GenerateSlug should produce valid slugs")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Product @ 123", "product-123")]
    [InlineData("Multiple   Spaces", "multiple-spaces")]
    public void GenerateSlug_ShouldProduceValidSlugs(string input, string expected)
    {
        var result = SluggableBehavior.GenerateSlug(input);

        result.Should().Be(expected);
    }
}
