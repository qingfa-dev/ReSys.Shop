using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Product")]
[Trait("Concern", "Availability")]
public class ProductMethodAvailabilityTests
{
    [Fact(DisplayName = "IsAvailable: Should return true when active and not deleted")]
    public void IsAvailable_WhenActive_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        product.IsAvailable().Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailable: Should return false when deleted")]
    public void IsAvailable_WhenDeleted_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.Delete("admin");

        product.IsAvailable().Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailable: Should return false when available-on is in the future")]
    public void IsAvailable_WhenFutureAvailableOn_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active, availableOn: DateTimeOffset.UtcNow.AddDays(1)).Value;

        product.IsAvailable().Should().BeFalse();
    }

    [Fact(DisplayName = "IsAvailable: Should return true when available-on is in the past")]
    public void IsAvailable_WhenPastAvailableOn_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active, availableOn: DateTimeOffset.UtcNow.AddDays(-1)).Value;

        product.IsAvailable().Should().BeTrue();
    }

    [Fact(DisplayName = "DefaultVariant: Should prefer non-master variant")]
    public void DefaultVariant_WithNonMaster_ShouldReturnNonMaster()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        var master = VariantMethod.Create(product.Id, "M", isMaster: true).Value;
        var nonMaster = VariantMethod.Create(product.Id, "V", isMaster: false).Value;
        product.Variants.Add(master);
        product.Variants.Add(nonMaster);

        var result = product.DefaultVariant();

        result.Should().Be(nonMaster);
    }

    [Fact(DisplayName = "DefaultVariant: Should fallback to master variant")]
    public void DefaultVariant_WithOnlyMaster_ShouldReturnMaster()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        var master = VariantMethod.Create(product.Id, "M", isMaster: true).Value;
        product.Variants.Add(master);

        var result = product.DefaultVariant();

        result.Should().Be(master);
    }

    [Fact(DisplayName = "DefaultVariant: Should return null when no variants")]
    public void DefaultVariant_WithNoVariants_ShouldReturnNull()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        var result = product.DefaultVariant();

        result.Should().BeNull();
    }

    [Fact(DisplayName = "HasVariants: Should return true when variants exist")]
    public void HasVariants_WithVariants_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;
        product.Variants.Add(VariantMethod.Create(product.Id, "V").Value);

        product.HasVariants().Should().BeTrue();
    }

    [Fact(DisplayName = "HasVariants: Should return false when no variants")]
    public void HasVariants_WithoutVariants_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "product", status: ProductStatus.Active).Value;

        product.HasVariants().Should().BeFalse();
    }
}
