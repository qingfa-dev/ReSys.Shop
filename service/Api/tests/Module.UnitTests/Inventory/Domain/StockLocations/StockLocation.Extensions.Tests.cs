using Module.Inventory.Domain.StockLocations;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockLocation")]
public class StockLocationExtensionsTests
{
    [Fact(DisplayName = "Create: Should return StockLocation with correct properties")]
    public void Create_WithValidParameters_ShouldReturnStockLocation()
    {
        var id = Guid.NewGuid();

        var result = StockLocationMethod.Create("Warehouse A", id: id);
        var location = result.Value;

        result.IsSuccess.Should().BeTrue();
        location.Id.Should().Be(id);
        location.Name.Should().Be("Warehouse A");
        location.Active.Should().Be(StockLocationConstant.Defaults.Active);
    }

    [Fact(DisplayName = "Update: Should update properties")]
    public void Update_WithValidParameters_ShouldUpdate()
    {
        var location = StockLocationMethod.Create("Old").Value;

        var result = location.Update(name: "New", active: true);

        result.IsSuccess.Should().BeTrue();
        location.Name.Should().Be("New");
    }

    [Fact(DisplayName = "Update: Cannot deactivate default location")]
    public void Update_WhenDeactivatingDefault_ShouldFail()
    {
        var location = StockLocationMethod.Create("Default", isDefault: true).Value;

        var result = location.Update(active: false);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockLocationResult.Errors.CannotDeactivateDefault);
    }

    [Fact(DisplayName = "Activate: Should activate inactive location")]
    public void Activate_WhenInactive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: false).Value;

        var result = location.Activate();

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Deactivate: Cannot deactivate default location")]
    public void Deactivate_WhenDefault_ShouldFail()
    {
        var location = StockLocationMethod.Create("Default", isDefault: true).Value;

        var result = location.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockLocationResult.Errors.CannotDeactivateDefault);
    }

    [Fact(DisplayName = "SoftDelete: Cannot delete active location")]
    public void SoftDelete_WhenActive_ShouldFail()
    {
        var location = StockLocationMethod.Create("Active", active: true).Value;

        var result = location.SoftDelete();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockLocationResult.Errors.CannotDeleteActive);
    }

    [Fact(DisplayName = "SoftDelete: Should delete inactive location")]
    public void SoftDelete_WhenInactive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Inactive", active: false).Value;

        var result = location.SoftDelete();

        result.IsSuccess.Should().BeTrue();
        location.IsDeleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Restore: Should restore deleted location")]
    public void Restore_WhenDeleted_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: false).Value;
        location.SoftDelete();

        var result = location.Restore();

        result.IsSuccess.Should().BeTrue();
        location.IsDeleted.Should().BeFalse();
    }

    [Fact(DisplayName = "StocksItem: Should return correct stock status")]
    public void StocksItem_ShouldReturnCorrectStatus()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new() { VariantId = variantId }];

        location.StocksItem(variantId).Should().BeTrue();
        location.StocksItem(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact(DisplayName = "FillStatus: Should return correct fill values")]
    public void FillStatus_ShouldReturnCorrectValues()
    {
        var location = StockLocationMethod.Create("Test").Value;
        var variantId = Guid.NewGuid();
        location.StockItems = [new() { VariantId = variantId, CountOnHand = 10, Backorderable = true }];

        var (onHand, backordered) = location.FillStatus(variantId, 5);

        onHand.Should().Be(5);
        backordered.Should().Be(0);
    }

    [Fact(DisplayName = "SetAsDefault: Should set default flag")]
    public void SetAsDefault_ShouldSetDefault()
    {
        var location = StockLocationMethod.Create("Test").Value;

        var result = location.SetAsDefault();

        result.IsSuccess.Should().BeTrue();
        location.Default.Should().BeTrue();
    }
}
