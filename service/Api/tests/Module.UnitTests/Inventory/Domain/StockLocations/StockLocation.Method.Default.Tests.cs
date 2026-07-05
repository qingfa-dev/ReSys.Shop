using Module.Inventory.Domain.StockLocations;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockLocation")]
public class StockLocationMethodDefaultTests
{
    [Fact(DisplayName = "SetAsDefault: Should set default flag")]
    public void SetAsDefault_WhenNotDefault_ShouldSetDefault()
    {
        var location = StockLocationMethod.Create("Test").Value;

        var result = location.SetAsDefault();

        result.IsSuccess.Should().BeTrue();
        location.Default.Should().BeTrue();
    }

    [Fact(DisplayName = "SetAsDefault: Should succeed when already default")]
    public void SetAsDefault_WhenAlreadyDefault_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", isDefault: true).Value;

        var result = location.SetAsDefault();

        result.IsSuccess.Should().BeTrue();
        location.Default.Should().BeTrue();
    }
}
