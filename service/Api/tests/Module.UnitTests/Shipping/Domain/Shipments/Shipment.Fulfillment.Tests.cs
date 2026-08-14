using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Shipments;

namespace Module.UnitTests.Shipping.Domain.Shipments;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Entity", "Shipment")]
public class ShipmentFulfillmentTests
{
    private static OrderFulfillmentState Compute(params ShipmentStatus[] statuses)
        => ShipmentMethod.ComputeFulfillmentState(statuses);

    [Fact(DisplayName = "Compute: empty -> None")]
    public void Empty_ReturnsNone()
        => Compute().Should().Be(OrderFulfillmentState.None);

    [Fact(DisplayName = "Compute: all Delivered -> Delivered")]
    public void AllDelivered_ReturnsDelivered()
        => Compute(ShipmentStatus.Delivered, ShipmentStatus.Delivered).Should().Be(OrderFulfillmentState.Delivered);

    [Fact(DisplayName = "Compute: all Canceled -> Canceled")]
    public void AllCanceled_ReturnsCanceled()
        => Compute(ShipmentStatus.Canceled, ShipmentStatus.Canceled).Should().Be(OrderFulfillmentState.Canceled);

    [Fact(DisplayName = "Compute: all Shipped -> Shipped")]
    public void AllShipped_ReturnsShipped()
        => Compute(ShipmentStatus.Shipped, ShipmentStatus.Shipped).Should().Be(OrderFulfillmentState.Shipped);

    [Fact(DisplayName = "Compute: mix of shipped/delivered and pending -> Partial")]
    public void MixedShippedAndPending_ReturnsPartial()
        => Compute(ShipmentStatus.Delivered, ShipmentStatus.Pending).Should().Be(OrderFulfillmentState.Partial);

    [Fact(DisplayName = "Compute: none shipped (pending/ready/backorder) -> Pending")]
    public void NoneShipped_ReturnsPending()
        => Compute(ShipmentStatus.Pending, ShipmentStatus.Ready, ShipmentStatus.Backorder).Should().Be(OrderFulfillmentState.Pending);
}
