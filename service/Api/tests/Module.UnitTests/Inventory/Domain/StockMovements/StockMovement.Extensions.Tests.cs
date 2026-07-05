using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.UnitTests.Inventory.Domain.StockMovements;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockMovement")]
public class StockMovementExtensionsTests
{
    [Fact(DisplayName = "Create: Should return StockMovement with correct properties")]
    public void Create_WithValidParameters_ShouldReturnStockMovement()
    {
        var stockItemId = Guid.NewGuid();
        var originatorId = Guid.NewGuid();

        var result = StockMovementExtensions.Create(stockItemId, 10, 100, "Order", originatorId, "Restock");
        var movement = result.Value;

        result.IsSuccess.Should().BeTrue();
        movement.StockItemId.Should().Be(stockItemId);
        movement.Quantity.Should().Be(10);
        movement.PreviousCountOnHand.Should().Be(100);
        movement.OriginatorType.Should().Be("Order");
        movement.OriginatorId.Should().Be(originatorId);
        movement.Reason.Should().Be("Restock");
    }

    [Fact(DisplayName = "Create: Should fail when quantity is zero")]
    public void Create_WithZeroQuantity_ShouldFail()
    {
        var result = StockMovementExtensions.Create(Guid.NewGuid(), 0);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockMovementResult.Errors.QuantityZero);
    }

    [Fact(DisplayName = "Create: Should fail with invalid originator type")]
    public void Create_WithInvalidOriginatorType_ShouldFail()
    {
        var result = StockMovementExtensions.Create(Guid.NewGuid(), 5, originatorType: "InvalidType");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockMovementResult.Errors.InvalidOriginatorType);
    }

    [Fact(DisplayName = "Create: Should accept valid originator types")]
    public void Create_WithValidOriginatorTypes_ShouldSucceed()
    {
        var move1 = StockMovementExtensions.Create(Guid.NewGuid(), 5, originatorType: "Order").Value;
        move1.OriginatorType.Should().Be("Order");

        var move2 = StockMovementExtensions.Create(Guid.NewGuid(), 5, originatorType: "Transfer").Value;
        move2.OriginatorType.Should().Be("Transfer");

        var move3 = StockMovementExtensions.Create(Guid.NewGuid(), 5, originatorType: "Adjustment").Value;
        move3.OriginatorType.Should().Be("Adjustment");
    }
}
