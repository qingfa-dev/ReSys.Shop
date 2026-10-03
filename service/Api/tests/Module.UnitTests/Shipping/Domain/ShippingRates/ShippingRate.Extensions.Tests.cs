using Module.Shipping.Domain.ShippingRates;
namespace Module.UnitTests.Shipping.Domain.ShippingRates;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","ShippingRate")]
public class ShippingRateExtensionsTests
{
    [Fact]
    public void Create_WithValidParams_ShouldReturnRate()
    {
        var methodId = Guid.NewGuid();
        var r = ShippingRateMethod.Create("Standard", 9.99m, methodId);
        r.IsSuccess.Should().BeTrue();
        r.Value.Name.Should().Be("Standard");
        r.Value.Cost.Should().Be(9.99m);
        r.Value.FinalPrice.Should().Be(9.99m);
        r.Value.Selected.Should().BeFalse();
        r.Value.ShippingMethodId.Should().Be(methodId);
    }
    [Fact]
    public void Create_WithZeroCost_ShouldFail()
    {
        var r = ShippingRateMethod.Create("Free", 0, Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Create_WithNegativeCost_ShouldFail()
    {
        var r = ShippingRateMethod.Create("Neg", -5, Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Select_WhenNotSelected_ShouldSucceed()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        r.Selected.Should().BeFalse();
        var res = r.Select();
        res.IsSuccess.Should().BeTrue();
        r.Selected.Should().BeTrue();
    }
    [Fact]
    public void Select_WhenAlreadySelected_ShouldFail()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        r.Select();
        var res = r.Select();
        res.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Unselect_WhenSelected_ShouldSucceed()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        r.Select();
        var res = r.Unselect();
        res.IsSuccess.Should().BeTrue();
        r.Selected.Should().BeFalse();
    }
    [Fact]
    public void Unselect_WhenNotSelected_ShouldFail()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        var res = r.Unselect();
        res.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void IsFree_WhenCostZero_ShouldReturnTrue()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        r.Cost = 0;
        r.IsFree().Should().BeTrue();
    }
    [Fact]
    public void IsFree_WhenCostNegative_ShouldReturnTrue()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        r.Cost = -1;
        r.IsFree().Should().BeTrue();
    }
    [Fact]
    public void IsFree_WhenCostPositive_ShouldReturnFalse()
    {
        var r = ShippingRateMethod.Create("Std", 10, Guid.NewGuid()).Value;
        r.IsFree().Should().BeFalse();
    }
}
