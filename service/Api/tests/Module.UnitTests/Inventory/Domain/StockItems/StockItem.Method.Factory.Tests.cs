using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockItems;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockItem")]
public class StockItemMethodFactoryTests
{
    [Fact(DisplayName = "Create: Should return StockItem with valid count-on-hand")]
    public void Create_WithValidCountOnHand_ShouldSucceed()
    {
        var result = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: 10);
        var item = result.Value;

        result.IsSuccess.Should().BeTrue();
        item.Id.Should().NotBeEmpty();
        item.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Create: Should fail with negative count-on-hand")]
    public void Create_WithNegativeCount_ShouldFail()
    {
        var result = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid(), countOnHand: -1);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockItemResult.Errors.NegativeCountOnHand);
    }

    [Fact(DisplayName = "Create: Should apply default values when not specified")]
    public void Create_WithDefaults_ShouldSetDefaultValues()
    {
        var result = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid());
        var item = result.Value;

        result.IsSuccess.Should().BeTrue();
        item.CountOnHand.Should().Be(StockItemConstant.Defaults.CountOnHand);
        item.Backorderable.Should().Be(StockItemConstant.Defaults.Backorderable);
    }

    [Fact(DisplayName = "Create: Should generate unique ids for different instances")]
    public void Create_WithoutExplicitId_ShouldGenerateUniqueIds()
    {
        var item1 = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var item2 = StockItemMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        item1.Id.Should().NotBe(item2.Id);
    }
}
