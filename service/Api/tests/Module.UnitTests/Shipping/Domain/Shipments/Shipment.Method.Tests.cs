using Module.Shipping.Domain.Shipments;

namespace Module.UnitTests.Shipping.Domain.Shipments;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Entity", "Shipment")]
public class ShipmentMethodTests
{
    private static Shipment CreateShipment()
        => ShipmentMethod.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

    [Fact(DisplayName = "Create: returns a Pending shipment")]
    public void Create_ReturnsPending()
    {
        var shipment = CreateShipment();
        shipment.Status.Should().Be(ShipmentStatus.Pending);
        shipment.OrderId.Should().NotBe(Guid.Empty);
        shipment.ShippingMethodId.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Create: rejects empty order id")]
    public void Create_EmptyOrderId_Fails()
    {
        ShipmentMethod.Create(Guid.Empty, Guid.NewGuid()).IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Create: rejects empty shipping method id")]
    public void Create_EmptyShippingMethodId_Fails()
    {
        ShipmentMethod.Create(Guid.NewGuid(), Guid.Empty).IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkReady: Pending -> Ready")]
    public void MarkReady_FromPending_Succeeds()
    {
        var shipment = CreateShipment();
        shipment.MarkReady().IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Ready);
    }

    [Fact(DisplayName = "MarkReady: Backorder -> Ready (restock)")]
    public void MarkReady_FromBackorder_Succeeds()
    {
        var shipment = CreateShipment();
        shipment.Backorder();
        shipment.MarkReady().IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Ready);
    }

    [Fact(DisplayName = "MarkReady: Shipped -> Ready fails")]
    public void MarkReady_FromShipped_Fails()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        shipment.MarkShipped("TRK123");
        shipment.MarkReady().IsFailure.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Shipped);
    }

    [Fact(DisplayName = "MarkShipped: Ready -> Shipped records tracking + timestamp")]
    public void MarkShipped_FromReady_Succeeds()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        var result = shipment.MarkShipped("TRK-001");
        result.IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Shipped);
        shipment.TrackingNumber.Should().Be("TRK-001");
        shipment.ShippedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "MarkShipped: rejects empty tracking number")]
    public void MarkShipped_EmptyTracking_Fails()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        shipment.MarkShipped("  ").IsFailure.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Ready);
    }

    [Fact(DisplayName = "MarkShipped: rejects non-Ready")]
    public void MarkShipped_NotReady_Fails()
    {
        var shipment = CreateShipment();
        shipment.MarkShipped("TRK").IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkDelivered: Shipped -> Delivered")]
    public void MarkDelivered_FromShipped_Succeeds()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        shipment.MarkShipped("TRK-001");
        shipment.MarkDelivered().IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Delivered);
        shipment.DeliveredAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "MarkDelivered: non-Shipped fails")]
    public void MarkDelivered_NotShipped_Fails()
    {
        var shipment = CreateShipment();
        shipment.MarkDelivered().IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Backorder: Pending -> Backorder")]
    public void Backorder_FromPending_Succeeds()
    {
        var shipment = CreateShipment();
        shipment.Backorder().IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Backorder);
    }

    [Fact(DisplayName = "Backorder: Ready -> Backorder fails")]
    public void Backorder_FromReady_Fails()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        shipment.Backorder().IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Cancel: Pending, Ready, and Backorder -> Canceled")]
    public void Cancel_FromPendingAndReady_Succeeds()
    {
        var pending = CreateShipment();
        pending.Cancel().IsSuccess.Should().BeTrue();
        pending.Status.Should().Be(ShipmentStatus.Canceled);

        var ready = CreateShipment();
        ready.MarkReady();
        ready.Cancel().IsSuccess.Should().BeTrue();
        ready.Status.Should().Be(ShipmentStatus.Canceled);

        var backorder = CreateShipment();
        backorder.Backorder();
        backorder.Cancel().IsSuccess.Should().BeTrue();
        backorder.Status.Should().Be(ShipmentStatus.Canceled);
    }

    [Fact(DisplayName = "Cancel: Shipped and Delivered fail")]
    public void Cancel_FromShippedAndDelivered_Fails()
    {
        var shipped = CreateShipment();
        shipped.MarkReady();
        shipped.MarkShipped("TRK");
        shipped.Cancel().IsFailure.Should().BeTrue();
        shipped.Status.Should().Be(ShipmentStatus.Shipped);

        var delivered = CreateShipment();
        delivered.MarkReady();
        delivered.MarkShipped("TRK");
        delivered.MarkDelivered();
        delivered.Cancel().IsFailure.Should().BeTrue();
        delivered.Status.Should().Be(ShipmentStatus.Delivered);
    }

    [Fact(DisplayName = "UpdateTrackingNumber: Ready accepts a replacement")]
    public void UpdateTrackingNumber_FromReady_Succeeds()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        shipment.UpdateTrackingNumber("NEW-TRK").IsSuccess.Should().BeTrue();
        shipment.TrackingNumber.Should().Be("NEW-TRK");
        shipment.ModifiedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "UpdateTrackingNumber: Shipped and Delivered accept a replacement")]
    public void UpdateTrackingNumber_FromShippedAndDelivered_Succeeds()
    {
        var shipped = CreateShipment();
        shipped.MarkReady();
        shipped.MarkShipped("OLD");
        shipped.UpdateTrackingNumber("NEW").IsSuccess.Should().BeTrue();
        shipped.TrackingNumber.Should().Be("NEW");

        var delivered = CreateShipment();
        delivered.MarkReady();
        delivered.MarkShipped("OLD");
        delivered.MarkDelivered();
        delivered.UpdateTrackingNumber("NEW").IsSuccess.Should().BeTrue();
        delivered.TrackingNumber.Should().Be("NEW");
    }

    [Fact(DisplayName = "UpdateTrackingNumber: Pending rejects a tracking number")]
    public void UpdateTrackingNumber_FromPending_Fails()
    {
        var shipment = CreateShipment();
        shipment.UpdateTrackingNumber("TRK").IsFailure.Should().BeTrue();
        shipment.TrackingNumber.Should().BeEmpty();
    }

    [Fact(DisplayName = "UpdateTrackingNumber: Canceled rejects a tracking number")]
    public void UpdateTrackingNumber_FromCanceled_Fails()
    {
        var shipment = CreateShipment();
        shipment.Cancel();
        shipment.UpdateTrackingNumber("TRK").IsFailure.Should().BeTrue();
        shipment.TrackingNumber.Should().BeEmpty();
    }

    [Fact(DisplayName = "UpdateTrackingNumber: empty value is rejected")]
    public void UpdateTrackingNumber_Empty_Fails()
    {
        var shipment = CreateShipment();
        shipment.MarkReady();
        shipment.UpdateTrackingNumber("  ").IsFailure.Should().BeTrue();
    }
}
