using Module.Catalog.Domain.Products;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Product")]
[Trait("Concern", "Status")]
public class ProductMethodStatusTests
{
    [Fact(DisplayName = "Activate: Should set status to Active")]
    public void Activate_WhenDraft_ShouldSetActive()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;

        var result = product.Activate();

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact(DisplayName = "Activate: When already active should return failure")]
    public void Activate_WhenAlreadyActive_ShouldReturnFailure()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        var result = product.Activate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(ProductResult.Errors.AlreadyActive);
    }

    [Fact(DisplayName = "Activate: When archived should return failure")]
    public void Activate_WhenArchived_ShouldReturnFailure()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Archived).Value;

        var result = product.Activate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(ProductResult.Errors.CannotActivateArchivedProduct);
    }

    [Fact(DisplayName = "Archive: Should set status to Archived")]
    public void Archive_WhenActive_ShouldSetArchived()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        var result = product.Archive();

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Archived);
        product.DiscontinueOn.Should().NotBeNull();
    }

    [Fact(DisplayName = "Archive: When already archived should return failure")]
    public void Archive_WhenAlreadyArchived_ShouldReturnFailure()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Archived).Value;

        var result = product.Archive();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(ProductResult.Errors.AlreadyArchived);
    }

    [Fact(DisplayName = "Draft: Should set status to Draft")]
    public void Draft_WhenActive_ShouldSetDraft()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        var result = product.Draft();

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact(DisplayName = "Draft: When already draft should return failure")]
    public void Draft_WhenAlreadyDraft_ShouldReturnFailure()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;

        var result = product.Draft();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(ProductResult.Errors.AlreadyDraft);
    }

    [Fact(DisplayName = "Discontinue: Should set DiscontinueOn and archive")]
    public void Discontinue_WhenActive_ShouldSetDiscontinueOn()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        var result = product.Discontinue();

        result.IsSuccess.Should().BeTrue();
        product.DiscontinueOn.Should().NotBeNull();
        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact(DisplayName = "Discontinue: When already discontinued should return failure")]
    public void Discontinue_WhenAlreadyDiscontinued_ShouldReturnFailure()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.DiscontinueOn = DateTimeOffset.UtcNow.AddDays(-1);

        var result = product.Discontinue();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(ProductResult.Errors.AlreadyDiscontinued);
    }
}
