using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockLocation")]
public class StockLocationMethodQueryTests
{
    [Fact(DisplayName = "StocksItem: Should return true when variant found")]
    public void StocksItem_WhenVariantFound_ShouldReturnTrue()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId }];

        location.StocksItem(variantId).Should().BeTrue();
    }

    [Fact(DisplayName = "StocksItem: Should return false when variant not found")]
    public void StocksItem_WhenVariantNotFound_ShouldReturnFalse()
    {
        var location = StockLocationMethod.Create("Test").Value;
        location.StockItems = [new StockItem { VariantId = Guid.NewGuid() }];

        location.StocksItem(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact(DisplayName = "StocksItem: Should return false when StockItems is null")]
    public void StocksItem_WhenStockItemsNull_ShouldReturnFalse()
    {
        var location = StockLocationMethod.Create("Test").Value;

        location.StocksItem(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact(DisplayName = "FillStatus: Should return full on_hand when sufficient stock")]
    public void FillStatus_WhenSufficientStock_ShouldReturnFullOnHand()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId, CountOnHand = 10, Backorderable = true }];

        var (onHand, backordered) = location.FillStatus(variantId, 5);

        onHand.Should().Be(5);
        backordered.Should().Be(0);
    }

    [Fact(DisplayName = "FillStatus: Should return partial with backordered when backorderable")]
    public void FillStatus_WhenPartialAndBackorderable_ShouldReturnBackordered()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId, CountOnHand = 3, Backorderable = true }];

        var (onHand, backordered) = location.FillStatus(variantId, 10);

        onHand.Should().Be(3);
        backordered.Should().Be(7);
    }

    [Fact(DisplayName = "FillStatus: Should return partial without backordered when not backorderable")]
    public void FillStatus_WhenPartialAndNotBackorderable_ShouldReturnZeroBackordered()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId, CountOnHand = 3, Backorderable = false }];

        var (onHand, backordered) = location.FillStatus(variantId, 10);

        onHand.Should().Be(3);
        backordered.Should().Be(0);
    }

    [Fact(DisplayName = "FillStatus: Should return zero when no stock item")]
    public void FillStatus_WhenNoStockItem_ShouldReturnZero()
    {
        var location = StockLocationMethod.Create("Test").Value;

        var (onHand, backordered) = location.FillStatus(Guid.NewGuid(), 5);

        onHand.Should().Be(0);
        backordered.Should().Be(0);
    }

    [Fact(DisplayName = "FillStatus: Should handle zero on_hand")]
    public void FillStatus_WhenZeroOnHand_ShouldReturnZeroOnHand()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId, CountOnHand = 0, Backorderable = true }];

        var (onHand, backordered) = location.FillStatus(variantId, 5);

        onHand.Should().Be(0);
        backordered.Should().Be(5);
    }

    [Fact(DisplayName = "FillStatus: Should handle negative on_hand as zero")]
    public void FillStatus_WhenNegativeOnHand_ShouldReturnZeroOnHand()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId, CountOnHand = -5, Backorderable = true }];

        var (onHand, backordered) = location.FillStatus(variantId, 10);

        onHand.Should().Be(0);
        backordered.Should().Be(10);
    }

    [Fact(DisplayName = "FillStatus: Should handle exact match")]
    public void FillStatus_WhenExactMatch_ShouldReturnFullOnHand()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new StockItem { VariantId = variantId, CountOnHand = 10, Backorderable = false }];

        var (onHand, backordered) = location.FillStatus(variantId, 10);

        onHand.Should().Be(10);
        backordered.Should().Be(0);
    }
}
