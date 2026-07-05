using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockItems;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockItem")]
public class StockItemExtensionsTests
{
    [Fact(DisplayName = "Create: Should return StockItem with valid count-on-hand")]
    public void Create_WithValidCountOnHand_ShouldSucceed()
    {
        var result = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10);
        var item = result.Value;

        result.IsSuccess.Should().BeTrue();
        item.Id.Should().NotBeEmpty();
        item.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Create: Should fail with negative count-on-hand")]
    public void Create_WithNegativeCount_ShouldFail()
    {
        var result = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: -1);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
    }

    [Fact(DisplayName = "AdjustCountOnHand: Should adjust correctly")]
    public void AdjustCountOnHand_WithValidQuantity_ShouldSucceed()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.AdjustCountOnHand(5);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "AdjustCountOnHand: Should fail if result is negative")]
    public void AdjustCountOnHand_WithNegativeResult_ShouldFail()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 5).Value;

        var result = item.AdjustCountOnHand(-10);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
        item.CountOnHand.Should().Be(5);
    }

    [Fact(DisplayName = "Restock: Should increase count-on-hand")]
    public void Restock_WithPositiveQuantity_ShouldSucceed()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.Restock(5);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(15);
    }

    [Fact(DisplayName = "Restock: Should fail with non-positive quantity")]
    public void Restock_WithNonPositive_ShouldFail()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = item.Restock(0);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Pick: Should decrease count-on-hand with sufficient stock")]
    public void Pick_WithSufficientStock_ShouldSucceed()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        var result = item.Pick(3);

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(7);
    }

    [Fact(DisplayName = "Pick: Should fail with insufficient stock")]
    public void Pick_WithInsufficientStock_ShouldFail()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 2).Value;

        var result = item.Pick(5);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.InsufficientStock);
    }

    [Fact(DisplayName = "SetBackorderable: Should update flag")]
    public void SetBackorderable_ShouldUpdate()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        item.Backorderable = false;

        var result = item.SetBackorderable(true);

        result.IsSuccess.Should().BeTrue();
        item.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailable: Should return true when sufficient stock")]
    public void IsAvailable_WithSufficientStock_ShouldReturnTrue()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        item.IsAvailable(5).Should().BeTrue();
    }

    [Fact(DisplayName = "IsAvailable: Should return true when backorderable")]
    public void IsAvailable_WhenBackorderable_ShouldReturnTrue()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 0).Value;
        item.Backorderable = true;

        item.IsAvailable(5).Should().BeTrue();
    }

    [Fact(DisplayName = "CanSupply: Should return correct value")]
    public void CanSupply_ShouldReturnCorrectValue()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10).Value;

        item.CanSupply(5).Should().BeTrue();
        item.CanSupply(15).Should().BeFalse();
    }

    [Fact(DisplayName = "TotalOnHand: Should return count-on-hand")]
    public void TotalOnHand_ShouldReturnCountOnHand()
    {
        var item = StockItemExtensions.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 42).Value;

        item.TotalOnHand().Should().Be(42);
    }
}
