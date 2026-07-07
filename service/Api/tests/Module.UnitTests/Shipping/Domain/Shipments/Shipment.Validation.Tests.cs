using Module.Shipping.Domain.Shipments;
namespace Module.UnitTests.Shipping.Domain.Shipments;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","Shipment")]
public class ShipmentValidationTests
{
    [Theory]
    [InlineData(ShipmentState.Pending, ShipmentState.Ready, true)]
    [InlineData(ShipmentState.Pending, ShipmentState.Canceled, true)]
    [InlineData(ShipmentState.Ready, ShipmentState.Shipped, true)]
    [InlineData(ShipmentState.Ready, ShipmentState.Canceled, true)]
    [InlineData(ShipmentState.Pending, ShipmentState.Pending, false)]
    [InlineData(ShipmentState.Pending, ShipmentState.Shipped, false)]
    [InlineData(ShipmentState.Ready, ShipmentState.Ready, false)]
    [InlineData(ShipmentState.Ready, ShipmentState.Pending, false)]
    [InlineData(ShipmentState.Shipped, ShipmentState.Pending, false)]
    [InlineData(ShipmentState.Shipped, ShipmentState.Ready, false)]
    [InlineData(ShipmentState.Shipped, ShipmentState.Shipped, false)]
    [InlineData(ShipmentState.Shipped, ShipmentState.Canceled, false)]
    [InlineData(ShipmentState.Canceled, ShipmentState.Pending, false)]
    [InlineData(ShipmentState.Canceled, ShipmentState.Ready, false)]
    [InlineData(ShipmentState.Canceled, ShipmentState.Shipped, false)]
    [InlineData(ShipmentState.Canceled, ShipmentState.Canceled, false)]
    public void IsValidTransition_ShouldReturnExpected(ShipmentState current, ShipmentState next, bool expected)
    {
        ShipmentValidation.IsValidTransition(current, next).Should().Be(expected);
    }
}
