using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Product")]
[Trait("Concern", "Scopes")]
public class ProductMethodScopesTests
{
    [Fact(DisplayName = "IsDraft: Should return true when Draft")]
    public void IsDraft_WhenDraft_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;

        product.IsDraft().Should().BeTrue();
    }

    [Fact(DisplayName = "IsDraft: Should return false when Active")]
    public void IsDraft_WhenActive_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        product.IsDraft().Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive: Should return true when Active")]
    public void IsActive_WhenActive_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        product.IsActive().Should().BeTrue();
    }

    [Fact(DisplayName = "IsArchived: Should return true when Archived")]
    public void IsArchived_WhenArchived_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Archived).Value;

        product.IsArchived().Should().BeTrue();
    }

    [Fact(DisplayName = "IsPurchasable: Should return true when variants exist and not deleted")]
    public void IsPurchasable_WithActiveVariants_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.Variants.Add(VariantMethod.Create(product.Id, "V", isMaster: false).Value);

        product.IsPurchasable().Should().BeTrue();
    }

    [Fact(DisplayName = "IsPurchasable: Should return false when no variants")]
    public void IsPurchasable_WithoutVariants_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        product.IsPurchasable().Should().BeFalse();
    }

    [Fact(DisplayName = "IsInStock: Should return true when any variant not deleted")]
    public void IsInStock_WithNonDeletedVariants_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.Variants.Add(VariantMethod.Create(product.Id, "V").Value);

        product.IsInStock().Should().BeTrue();
    }

    [Fact(DisplayName = "IsInStock: Should return false when no variants")]
    public void IsInStock_WithoutVariants_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        product.IsInStock().Should().BeFalse();
    }

    [Fact(DisplayName = "IsBackorderable: Should return true when any variant has tracking disabled")]
    public void IsBackorderable_WithTrackInventoryFalse_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        var variant = VariantMethod.Create(product.Id, "V", isMaster: false).Value;
        variant.TrackInventory = false;
        product.Variants.Add(variant);

        product.IsBackorderable().Should().BeTrue();
    }

    [Fact(DisplayName = "ResolveStatus: Should return Archived when deleted")]
    public void ResolveStatus_WhenDeleted_ShouldReturnArchived()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.Delete("admin");

        var result = product.ResolveStatus();

        result.Should().Be(ProductStatus.Archived);
    }

    [Fact(DisplayName = "ResolveStatus: Should return actual status when not deleted")]
    public void ResolveStatus_WhenNotDeleted_ShouldReturnActualStatus()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Draft).Value;

        var result = product.ResolveStatus();

        result.Should().Be(ProductStatus.Draft);
    }
}
