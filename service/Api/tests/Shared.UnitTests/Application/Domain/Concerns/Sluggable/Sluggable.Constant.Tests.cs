using Shared.Application.Domain.Concerns.Sluggable;

namespace Shared.UnitTests.Application.Domain.Concerns.Sluggable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class SluggableConstantTests
{
    [Fact(DisplayName = "MaxSlugLength should be 255")]
    public void MaxSlugLength_ShouldBe255()
    {
        SluggableConstant.Constraints.MaxSlugLength.Should().Be(255);
    }

    [Fact(DisplayName = "Slug pattern should match expected regex")]
    public void SlugPattern_ShouldBeValidRegex()
    {
        SluggableConstant.Patterns.Slug.Should().Be("^[a-z0-9\\-]+$");
    }

    [Fact(DisplayName = "AllowedSearchFields should contain Slug")]
    public void AllowedSearchFields_ShouldContainSlug()
    {
        SluggableConstant.Feilds.AllowedSearchFields.Should().Contain("Slug");
    }

    [Fact(DisplayName = "AllowedSortFields should contain Slug")]
    public void AllowedSortFields_ShouldContainSlug()
    {
        SluggableConstant.Feilds.AllowedSortFields.Should().Contain("Slug");
    }

    [Fact(DisplayName = "AllowedFilterFields should contain Slug")]
    public void AllowedFilterFields_ShouldContainSlug()
    {
        SluggableConstant.Feilds.AllowedFilterFields.Should().Contain("Slug");
    }
}
