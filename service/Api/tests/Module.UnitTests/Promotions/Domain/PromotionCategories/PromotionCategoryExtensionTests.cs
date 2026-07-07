using FluentAssertions;
using Module.Promotions.Domain.PromotionCategories;

namespace Module.UnitTests.Promotions.Domain.PromotionCategories;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionCategoryExtensions")]
public class PromotionCategoryExtensionTests
{
    [Fact(DisplayName = "Create: Should set properties")]
    public void Create_ShouldSetProperties()
    {
        var result = PromotionCategoryExtensions.Create("Seasonal");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Seasonal");
        result.Value.Code.Should().BeNull();
        result.Value.Presentation.Should().BeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Create: Should set optional fields")]
    public void Create_ShouldSetOptionalFields()
    {
        var result = PromotionCategoryExtensions.Create("Seasonal", code: "SEASONAL", presentation: "Seasonal Promos");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("SEASONAL");
        result.Value.Presentation.Should().Be("Seasonal Promos");
    }

    [Fact(DisplayName = "Update: Should update only non-null")]
    public void Update_ShouldUpdateOnlyNonNull()
    {
        var result = PromotionCategoryExtensions.Create("Original", code: "OLD", presentation: "Old Pres");
        var cat = result.Value;

        cat.Update(name: "Updated").IsSuccess.Should().BeTrue();

        cat.Name.Should().Be("Updated");
        cat.Code.Should().Be("OLD");
        cat.Presentation.Should().Be("Old Pres");
    }

    [Fact(DisplayName = "Delete: Should mark soft delete")]
    public void Delete_ShouldMarkSoftDelete()
    {
        var result = PromotionCategoryExtensions.Create("Test");
        var cat = result.Value;

        var delResult = cat.Delete("admin");

        delResult.IsSuccess.Should().BeTrue();
        cat.IsDeleted.Should().BeTrue();
        cat.DeletedAtUtc.Should().NotBeNull();
        cat.DeletedBy.Should().Be("admin");
    }

    [Fact(DisplayName = "Delete: Should be idempotent")]
    public void Delete_ShouldBeIdempotent()
    {
        var result = PromotionCategoryExtensions.Create("Test");
        var cat = result.Value;

        cat.Delete("admin");
        var deletedAt = cat.DeletedAtUtc;

        var delResult = cat.Delete("admin");
        delResult.IsSuccess.Should().BeTrue();
        cat.DeletedAtUtc.Should().Be(deletedAt);
        cat.DeletedBy.Should().Be("admin");
    }
}
