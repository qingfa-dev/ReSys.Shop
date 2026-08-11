using Module.Inventory.Domain.StockLocations;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockLocation")]
public class StockLocationMethodActivationTests
{
    [Fact(DisplayName = "Activate: Should activate inactive location")]
    public void Activate_WhenInactive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: false).Value;

        var result = location.Activate();

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Activate: Should succeed when already active")]
    public void Activate_WhenAlreadyActive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: true).Value;

        var result = location.Activate();

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Deactivate: Should deactivate active location")]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: true).Value;

        var result = location.Deactivate();

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: Should succeed when already inactive")]
    public void Deactivate_WhenAlreadyInactive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: false).Value;

        var result = location.Deactivate();

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: Cannot deactivate default location")]
    public void Deactivate_WhenDefault_ShouldFail()
    {
        var location = StockLocationMethod.Create("Default", isDefault: true).Value;

        var result = location.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockLocationResult.Errors.CannotDeactivateDefault);
        location.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Should update properties")]
    public void Update_WithValidParameters_ShouldUpdate()
    {
        var location = StockLocationMethod.Create("Old").Value;

        var result = location.Update(name: "New", active: true);

        result.IsSuccess.Should().BeTrue();
        location.Name.Should().Be("New");
    }

    [Fact(DisplayName = "Update: Should delegate activation via active parameter")]
    public void Update_WhenActivating_ShouldDelegateToActivate()
    {
        var location = StockLocationMethod.Create("Test", active: false).Value;

        var result = location.Update(active: true);

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Update: Should delegate deactivation via active parameter")]
    public void Update_WhenDeactivatingNonDefault_ShouldDelegateToDeactivate()
    {
        var location = StockLocationMethod.Create("Test", active: true).Value;

        var result = location.Update(active: false);

        result.IsSuccess.Should().BeTrue();
        location.Active.Should().BeFalse();
    }

    [Fact(DisplayName = "Update: Cannot deactivate default location")]
    public void Update_WhenDeactivatingDefault_ShouldFail()
    {
        var location = StockLocationMethod.Create("Default", isDefault: true).Value;

        var result = location.Update(active: false);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockLocationResult.Errors.CannotDeactivateDefault);
        location.Active.Should().BeTrue();
    }
}
