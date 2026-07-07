using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingRates;
namespace Module.UnitTests.Shipping.Domain.Shipments;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","Shipment")]
public class ShipmentExtensionsTests
{
    [Fact]
    public void Create_WithValidParams_ShouldReturnShipment()
    {
        var orderId = Guid.NewGuid();
        var locId = Guid.NewGuid();
        var r = ShipmentExtensions.Create(orderId, locId);
        r.IsSuccess.Should().BeTrue();
        r.Value.OrderId.Should().Be(orderId);
        r.Value.StockLocationId.Should().Be(locId);
        r.Value.State.Should().Be(ShipmentState.Pending);
    }
    [Fact]
    public void Ready_FromPending_ShouldTransition()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var r = s.Ready();
        r.IsSuccess.Should().BeTrue();
        s.State.Should().Be(ShipmentState.Ready);
    }
    [Fact]
    public void Ready_FromShipped_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Ship("TRACK123");
        var r = s.Ready();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Ship_WithTracking_ShouldTransition()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        var r = s.Ship("TRACK123");
        r.IsSuccess.Should().BeTrue();
        s.State.Should().Be(ShipmentState.Shipped);
        s.Tracking.Should().Be("TRACK123");
        s.ShippedAtUtc.Should().NotBeNull();
    }
    [Fact]
    public void Ship_WithoutTracking_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        var r = s.Ship("");
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Cancel_FromPending_ShouldSucceed()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var r = s.Cancel();
        r.IsSuccess.Should().BeTrue();
        s.State.Should().Be(ShipmentState.Canceled);
    }
    [Fact]
    public void Cancel_FromReady_ShouldSucceed()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        var r = s.Cancel();
        r.IsSuccess.Should().BeTrue();
        s.State.Should().Be(ShipmentState.Canceled);
    }
    [Fact]
    public void Cancel_FromShipped_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Ship("TRACK");
        var r = s.Cancel();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void RefreshRates_WhenNotShippedOrCanceled_ShouldClear()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.ShippingRates.Add(new ShippingRate { Id = Guid.NewGuid(), Name = "Rate1", Cost = 10 });
        var r = s.RefreshRates();
        r.IsSuccess.Should().BeTrue();
        s.ShippingRates.Should().BeEmpty();
    }
    [Fact]
    public void RefreshRates_WhenShipped_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Ship("TRACK");
        var r = s.RefreshRates();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void RefreshRates_WhenCanceled_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Cancel();
        var r = s.RefreshRates();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void SelectShippingRate_WithValidRate_ShouldSucceed()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var rateId = Guid.NewGuid();
        var methodId = Guid.NewGuid();
        s.ShippingRates.Add(new ShippingRate { Id = rateId, Name = "Std", Cost = 10, FinalPrice = 10, ShippingMethodId = methodId });
        var r = s.SelectShippingRate(rateId);
        r.IsSuccess.Should().BeTrue();
        s.ShippingMethodId.Should().Be(methodId);
        s.Cost.Should().Be(10);
        s.FinalPrice.Should().Be(10);
    }
    [Fact]
    public void SelectShippingRate_WithNoRates_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var r = s.SelectShippingRate(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void SelectShippingRate_WithUnknownRate_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.ShippingRates.Add(new ShippingRate { Id = Guid.NewGuid(), Name = "Std", Cost = 10 });
        var r = s.SelectShippingRate(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void TransferToLocation_WhenNotShippedOrCanceled_ShouldWork()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var newLoc = Guid.NewGuid();
        var r = s.TransferToLocation(newLoc);
        r.IsSuccess.Should().BeTrue();
        s.StockLocationId.Should().Be(newLoc);
    }
    [Fact]
    public void TransferToLocation_WhenShipped_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Ship("TRACK");
        var r = s.TransferToLocation(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void TransferToLocation_WhenCanceled_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Cancel();
        var r = s.TransferToLocation(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }
}
