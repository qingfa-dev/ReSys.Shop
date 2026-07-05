using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockItems;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockItem")]
public class StockItemMethodAdjustmentTests
{
    [Fact(DisplayName = "Update: Should update count-on-hand and backorderable")]
    public void Update_WithValidParameters_ShouldSucceed()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 5).Value;

        var result = item.Update(countOnHand: 10, backorderable: true);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(10);
        item.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Should fail with negative count-on-hand")]
    public void Update_WithNegativeCountOnHand_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 5).Value;

        var result = item.Update(countOnHand: -1);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
        item.CountOnHand.Should().Be(5);
    }

    [Fact(DisplayName = "AdjustCountOnHand: Should add quantity")]
    public void AdjustCountOnHand_WithPositiveQuantity_ShouldIncrease()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.AdjustCountOnHand(5);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "AdjustCountOnHand: Should subtract quantity")]
    public void AdjustCountOnHand_WithNegativeQuantity_ShouldDecrease()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.AdjustCountOnHand(-3);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(7);
    }

    [Fact(DisplayName = "AdjustCountOnHand: Should fail if result is negative")]
    public void AdjustCountOnHand_WithNegativeResult_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 5).Value;

        var result = item.AdjustCountOnHand(-10);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
        item.CountOnHand.Should().Be(5);
    }

    [Fact(DisplayName = "SetBackorderable: Should set to true")]
    public void SetBackorderable_ToTrue_ShouldUpdate()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), backorderable: false).Value;

        var result = item.SetBackorderable(true);

        result.IsSuccess.Should().BeTrue();
        item.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "SetBackorderable: Should set to false")]
    public void SetBackorderable_ToFalse_ShouldUpdate()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), backorderable: true).Value;

        var result = item.SetBackorderable(false);

        result.IsSuccess.Should().BeTrue();
        item.Backorderable.Should().BeFalse();
    }

    [Fact(DisplayName = "Restock: Should increase count-on-hand")]
    public void Restock_WithPositiveQuantity_ShouldSucceed()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.Restock(5);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "Restock: Should fail with zero")]
    public void Restock_WithZero_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = item.Restock(0);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
    }

    [Fact(DisplayName = "Restock: Should fail with negative")]
    public void Restock_WithNegative_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = item.Restock(-5);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
    }

    [Fact(DisplayName = "Pick: Should decrease count-on-hand with sufficient stock")]
    public void Pick_WithSufficientStock_ShouldSucceed()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.Pick(3);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(7);
    }

    [Fact(DisplayName = "Pick: Should succeed with exact stock")]
    public void Pick_WithExactStock_ShouldSucceed()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 5).Value;

        var result = item.Pick(5);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(0);
    }

    [Fact(DisplayName = "Pick: Should fail with insufficient stock")]
    public void Pick_WithInsufficientStock_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 2).Value;

        var result = item.Pick(5);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.InsufficientStock);
        item.CountOnHand.Should().Be(2);
    }

    [Fact(DisplayName = "Pick: Should fail with zero quantity")]
    public void Pick_WithZero_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.Pick(0);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
        item.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Pick: Should fail with negative quantity")]
    public void Pick_WithNegative_ShouldFail()
    {
        var item = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.Pick(-3);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
        item.CountOnHand.Should().Be(10);
    }
}
