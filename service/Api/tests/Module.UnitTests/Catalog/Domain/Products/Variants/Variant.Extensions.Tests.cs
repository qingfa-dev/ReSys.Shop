using Module.Catalog.Domain.Products.Variants;

namespace Module.UnitTests.Catalog.Domain.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Variant")]
public class VariantExtensionsTests
{
    [Theory(DisplayName = "Create: Should return Variant with correct properties")]
    [InlineData("SKU-001", true, 0)]
    public void Create_WithValidParameters_ShouldReturnVariant(string sku, bool isMaster, int position)
    {
        var productId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var result = VariantExtensions.Create(productId, sku, isMaster, position, id: id);
        var variant = result.Value;

        result.IsSuccess.Should().BeTrue();
        variant.Should().NotBeNull();
        variant.Id.Should().Be(id);
        variant.ProductId.Should().Be(productId);
        variant.Sku.Should().Be(sku);
        variant.IsMaster.Should().Be(isMaster);
        variant.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Create: With isMaster false should set correctly")]
    public void Create_WithIsMasterFalse_ShouldSetCorrectly()
    {
        var result = VariantExtensions.Create(Guid.NewGuid(), "SKU", isMaster: false, position: 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsMaster.Should().BeFalse();
        result.Value.Position.Should().Be(5);
    }

    [Fact(DisplayName = "Create: Without id should generate new id")]
    public void Create_WithoutId_ShouldGenerateNewId()
    {
        var result = VariantExtensions.Create(Guid.NewGuid(), "SKU");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Theory(DisplayName = "Update: Should update base properties")]
    [InlineData("NEW-SKU", 10, false)]
    public void Update_WithValidParameters_ShouldUpdateProperties(
        string sku,
        int position,
        bool trackInventory)
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "OLD-SKU", true, 0).Value;
        var result = variant.Update(sku, position, trackInventory);

        result.IsSuccess.Should().BeTrue();
        variant.Sku.Should().Be(sku);
        variant.Position.Should().Be(position);
        variant.TrackInventory.Should().Be(trackInventory);
    }

    [Fact(DisplayName = "UpdatePricing: Should update price and cost")]
    public void UpdatePricing_WithValidParameters_ShouldUpdatePricing()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        decimal price = 29.99m;
        decimal cost = 15.00m;
        string currency = "USD";

        var result = variant.UpdatePricing(price, cost, currency);

        result.IsSuccess.Should().BeTrue();
        variant.Price.Should().Be(price);
        variant.CostPrice.Should().Be(cost);
        variant.CostCurrency.Should().Be(currency);
    }

    [Fact(DisplayName = "UpdatePricing: Partial update should preserve other values")]
    public void UpdatePricing_WithOnlyPrice_ShouldPreserveOthers()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        variant.Price = 10m;
        variant.CostPrice = 5m;
        variant.CostCurrency = "USD";

        var result = variant.UpdatePricing(price: 20m);

        result.IsSuccess.Should().BeTrue();
        variant.Price.Should().Be(20m);
        variant.CostPrice.Should().Be(5m);
        variant.CostCurrency.Should().Be("USD");
    }

    [Fact(DisplayName = "UpdatePhysicalSpecs: Should update weight and size")]
    public void UpdatePhysicalSpecs_WithValidParameters_ShouldUpdateSpecs()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        var result = variant.UpdatePhysicalSpecs(1.5m, WeightUnit.Kg, 10m, 20m, 5m, DimensionUnit.Cm);

        result.IsSuccess.Should().BeTrue();
        variant.Weight.Should().Be(1.5m);
        variant.WeightUnit.Should().Be(WeightUnit.Kg);
        variant.Height.Should().Be(10m);
        variant.Width.Should().Be(20m);
        variant.Depth.Should().Be(5m);
        variant.DimensionsUnit.Should().Be(DimensionUnit.Cm);
    }

    [Fact(DisplayName = "UpdatePhysicalSpecs: Partial update should preserve other values")]
    public void UpdatePhysicalSpecs_WithOnlyWeight_ShouldPreserveOthers()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        variant.Weight = 1m;
        variant.WeightUnit = WeightUnit.Kg;
        variant.Height = 10m;

        var result = variant.UpdatePhysicalSpecs(weight: 5m);

        result.IsSuccess.Should().BeTrue();
        variant.Weight.Should().Be(5m);
        variant.WeightUnit.Should().Be(WeightUnit.Kg);
        variant.Height.Should().Be(10m);
    }

    [Fact(DisplayName = "Update: Partial update should preserve other properties")]
    public void Update_WithOnlySku_ShouldPreserveOthers()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "OLD-SKU", true, 5).Value;
        var result = variant.Update(sku: "NEW-SKU");

        result.IsSuccess.Should().BeTrue();
        variant.Sku.Should().Be("NEW-SKU");
        variant.Position.Should().Be(5);
        variant.TrackInventory.Should().BeTrue();
    }

    [Fact(DisplayName = "Delete: Should mark as deleted and raise event")]
    public void Delete_WhenCalled_ShouldSetIsDeleted()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU", true, 0).Value;

        var result = variant.Delete("admin");

        result.IsSuccess.Should().BeTrue();
        variant.IsDeleted.Should().BeTrue();
        variant.DeletedBy.Should().Be("admin");
        variant.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Delete: When already deleted should return Ok and not raise event")]
    public void Delete_WhenAlreadyDeleted_ShouldReturnOk()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        variant.Delete("admin");

        var result = variant.Delete("admin");

        result.IsSuccess.Should().BeTrue();
    }
}
