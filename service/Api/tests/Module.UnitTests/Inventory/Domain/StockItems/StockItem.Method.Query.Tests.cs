using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockItems;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockItem")]
public class StockItemMethodQueryTests
{
    [Fact(DisplayName = "FillStatus: Should return full on_hand when sufficient stock")]
    public void FillStatus_WhenSufficientStock_ShouldReturnFullOnHand()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var (onHand, backordered) = item.FillStatus(5);

        onHand.Should().Be(5);
        backordered.Should().Be(0);
    }

    [Fact(DisplayName = "FillStatus: Should return partial with backordered when backorderable")]
    public void FillStatus_WhenPartialAndBackorderable_ShouldReturnBackordered()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 3, backorderable: true).Value;

        var (onHand, backordered) = item.FillStatus(10);

        onHand.Should().Be(3);
        backordered.Should().Be(7);
    }

    [Fact(DisplayName = "FillStatus: Should return partial without backordered when not backorderable")]
    public void FillStatus_WhenPartialAndNotBackorderable_ShouldReturnZeroBackordered()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 3, backorderable: false).Value;

        var (onHand, backordered) = item.FillStatus(10);

        onHand.Should().Be(3);
        backordered.Should().Be(0);
    }

    [Fact(DisplayName = "ProcessBackorders: Should return full quantity when sufficient stock")]
    public void ProcessBackorders_WhenSufficientStock_ShouldReturnFullQuantity()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var remaining = item.ProcessBackorders(5);

        remaining.Should().Be(5);
    }

    [Fact(DisplayName = "ProcessBackorders: Should handle insufficient stock with backorderable")]
    public void ProcessBackorders_WhenInsufficientAndBackorderable_ShouldReturnRemaining()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 3, backorderable: true).Value;

        var remaining = item.ProcessBackorders(10);

        remaining.Should().Be(0);
    }

    [Fact(DisplayName = "ProcessBackorders: Should handle insufficient stock without backorderable")]
    public void ProcessBackorders_WhenInsufficientAndNotBackorderable_ShouldReturnUnfilled()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 3, backorderable: false).Value;

        var remaining = item.ProcessBackorders(10);

        remaining.Should().Be(7);
    }

    [Fact(DisplayName = "ProcessBackorders: Should return zero when quantity is zero")]
    public void ProcessBackorders_WithZeroQuantity_ShouldReturnZero()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        var remaining = item.ProcessBackorders(0);

        remaining.Should().Be(0);
    }
}
