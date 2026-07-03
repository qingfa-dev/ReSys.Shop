using Module.Catalog.Domain.Products.Variants;

namespace Module.UnitTests.Catalog.Domain.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Variant")]
[Trait("Concern", "Pricing")]
public class VariantMethodPricingTests
{
    [Fact(DisplayName = "UpdatePricing: Should update price and cost")]
    public void UpdatePricing_WithValidParameters_ShouldUpdatePricing()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU", true, 0).Value;
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
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU", true, 0).Value;
        variant.Price = 10m;
        variant.CostPrice = 5m;
        variant.CostCurrency = "USD";

        var result = variant.UpdatePricing(price: 20m);

        result.IsSuccess.Should().BeTrue();
        variant.Price.Should().Be(20m);
        variant.CostPrice.Should().Be(5m);
        variant.CostCurrency.Should().Be("USD");
    }
}
