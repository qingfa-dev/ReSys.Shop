using Module.Ordering.Domain.Adjustments;

namespace Module.UnitTests.Ordering.Domain.Adjustments;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Entity", "Adjustment")]
public class AdjustmentExtensionsTests
{
    [Fact]
    public void Create_ShouldReturnAdjustment()
    {
        var r = AdjustmentMethod.Create("Tax", 10.5m, Guid.NewGuid(), "Order", Guid.NewGuid(), "TaxRate", Guid.NewGuid());
        r.IsSuccess.Should().BeTrue();
        r.Value.Label.Should().Be("Tax"); r.Value.Amount.Should().Be(10.5m); r.Value.State.Should().Be("open");
    }
    [Fact]
    public void Close_WhenOpen_ShouldSucceed()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "TaxRate", Guid.NewGuid()).Value;
        var r = a.Close(); r.IsSuccess.Should().BeTrue(); a.State.Should().Be("closed");
    }
    [Fact]
    public void Close_WhenClosed_ShouldFail()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "TaxRate", Guid.NewGuid()).Value;
        a.Close(); var r = a.Close();
        r.IsFailure.Should().BeTrue(); r.FirstFailure.Should().Be(AdjustmentResult.Errors.AlreadyClosed);
    }
    [Fact]
    public void Open_WhenClosed_ShouldSucceed()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "TaxRate", Guid.NewGuid()).Value;
        a.Close(); var r = a.Open(); r.IsSuccess.Should().BeTrue(); a.State.Should().Be("open");
    }
    [Fact]
    public void Open_WhenOpen_ShouldFail()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "TaxRate", Guid.NewGuid()).Value;
        var r = a.Open();
        r.IsFailure.Should().BeTrue(); r.FirstFailure.Should().Be(AdjustmentResult.Errors.AlreadyOpen);
    }
}
