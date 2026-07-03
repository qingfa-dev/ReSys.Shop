using Module.Catalog.Domain.Products.Variants;

namespace Module.UnitTests.Catalog.Domain.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Variant")]
[Trait("Concern", "Status")]
public class VariantMethodStatusTests
{
    private static Variant CreateVariant() => new()
    {
        Id = Guid.NewGuid(),
        IsDeleted = false,
        DiscontinuedOn = null
    };

    [Fact(DisplayName = "IsPublished: Should return true when active")]
    public void IsPublished_WhenActive_ShouldReturnTrue()
    {
        var variant = CreateVariant();

        variant.IsPublished().Should().BeTrue();
    }

    [Fact(DisplayName = "IsPublished: Should return false when deleted")]
    public void IsPublished_WhenDeleted_ShouldReturnFalse()
    {
        var variant = CreateVariant();
        variant.IsDeleted = true;

        variant.IsPublished().Should().BeFalse();
    }

    [Fact(DisplayName = "IsPublished: Should return false when discontinued in the past")]
    public void IsPublished_WhenDiscontinued_ShouldReturnFalse()
    {
        var variant = CreateVariant();
        variant.DiscontinuedOn = DateTimeOffset.UtcNow.AddDays(-1);

        variant.IsPublished().Should().BeFalse();
    }

    [Fact(DisplayName = "IsPublished: Should return true when discontinued in the future")]
    public void IsPublished_WhenDiscontinuedInFuture_ShouldReturnTrue()
    {
        var variant = CreateVariant();
        variant.DiscontinuedOn = DateTimeOffset.UtcNow.AddDays(1);

        variant.IsPublished().Should().BeTrue();
    }

    [Fact(DisplayName = "Publish: Should clear DiscontinuedOn")]
    public void Publish_WhenUnpublished_ShouldClearDiscontinuedOn()
    {
        var variant = CreateVariant();
        variant.DiscontinuedOn = DateTimeOffset.UtcNow.AddDays(-1);

        var result = variant.Publish();

        result.IsSuccess.Should().BeTrue();
        variant.DiscontinuedOn.Should().BeNull();
    }

    [Fact(DisplayName = "Publish: When deleted should return failure")]
    public void Publish_WhenDeleted_ShouldReturnFailure()
    {
        var variant = CreateVariant();
        variant.IsDeleted = true;

        var result = variant.Publish();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(VariantResult.Errors.AlreadyDeleted);
    }

    [Fact(DisplayName = "Publish: When already published should return Ok")]
    public void Publish_WhenAlreadyPublished_ShouldReturnOk()
    {
        var variant = CreateVariant();

        var result = variant.Publish();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Discontinue: Should set DiscontinuedOn")]
    public void Discontinue_WhenActive_ShouldSetDiscontinuedOn()
    {
        var variant = CreateVariant();

        var result = variant.Discontinue();

        result.IsSuccess.Should().BeTrue();
        variant.DiscontinuedOn.Should().NotBeNull();
    }

    [Fact(DisplayName = "Discontinue: When already discontinued should return failure")]
    public void Discontinue_WhenAlreadyDiscontinued_ShouldReturnFailure()
    {
        var variant = CreateVariant();
        variant.DiscontinuedOn = DateTimeOffset.UtcNow.AddDays(-1);

        var result = variant.Discontinue();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(VariantResult.Errors.AlreadyDiscontinued);
    }
}
