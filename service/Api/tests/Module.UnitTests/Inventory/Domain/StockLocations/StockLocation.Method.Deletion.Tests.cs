using Module.Inventory.Domain.StockLocations;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Entity", "StockLocation")]
public class StockLocationMethodDeletionTests
{
    [Fact(DisplayName = "SoftDelete: Cannot delete active location")]
    public void SoftDelete_WhenActive_ShouldFail()
    {
        var location = StockLocationMethod.Create("Active", active: true).Value;

        var result = location.SoftDelete();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(StockLocationResult.Failure.CannotDeleteActive);
        location.IsDeleted.Should().BeFalse();
    }

    [Fact(DisplayName = "SoftDelete: Should delete inactive location")]
    public void SoftDelete_WhenInactive_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Inactive", active: false).Value;

        var result = location.SoftDelete();

        result.IsSuccess.Should().BeTrue();
        location.IsDeleted.Should().BeTrue();
        location.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "SoftDelete: Should succeed when already deleted")]
    public void SoftDelete_WhenAlreadyDeleted_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Inactive", active: false).Value;
        location.SoftDelete();

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
        location.DeletedAtUtc.Should().BeNull();
        location.DeletedBy.Should().BeNull();
    }

    [Fact(DisplayName = "Restore: Should succeed when not deleted")]
    public void Restore_WhenNotDeleted_ShouldSucceed()
    {
        var location = StockLocationMethod.Create("Test", active: false).Value;

        var result = location.Restore();

        result.IsSuccess.Should().BeTrue();
        location.IsDeleted.Should().BeFalse();
    }
}
