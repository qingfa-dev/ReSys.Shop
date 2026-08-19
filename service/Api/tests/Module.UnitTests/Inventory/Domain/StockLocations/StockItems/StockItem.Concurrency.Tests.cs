using Module.Inventory.Domain.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockLocations.StockItems;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItem.Concurrency")]
public class StockItemConcurrencyTests
{
    [Fact(DisplayName = "ConcurrencyConflict: Returns Error with Conflict status")]
    public void ConcurrencyConflict_ShouldReturnConflictError()
    {
        var variantId = Guid.NewGuid();

        var error = StockItemResult.Errors.ConcurrencyConflict(variantId);

        error.Type.Should().Be(409);
        error.Code.Should().Be("StockItem.ConcurrencyConflict");
        error.Message.Should().Contain(variantId.ToString());
    }
}
