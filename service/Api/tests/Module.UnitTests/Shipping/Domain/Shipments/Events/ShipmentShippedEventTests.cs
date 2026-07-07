using FluentAssertions;
using Module.Shipping.Domain.Shipments.Events;
using Xunit;

namespace Module.UnitTests.Shipping.Domain.Shipments.Events;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "DomainEvents")]
public sealed class ShipmentShippedEventTests
{
    [Fact(DisplayName = "ShipmentShippedEvent should initialize with correct data")]
    public void Constructor_ShouldInitializeWithCorrectData()
    {
        var shipmentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var tracking = "TRACK123456";
        var carrier = "UPS";
        var shippedAt = DateTimeOffset.UtcNow;
        var customerId = Guid.NewGuid();
        var customerEmail = "customer@example.com";

        var e = new ShipmentEvent.Lifecycle.Shipped(shipmentId, orderId, tracking, carrier, shippedAt, customerId, customerEmail);

        e.ShipmentId.Should().Be(shipmentId);
        e.OrderId.Should().Be(orderId);
        e.TrackingNumber.Should().Be(tracking);
        e.Carrier.Should().Be(carrier);
        e.ShippedAtUtc.Should().Be(shippedAt);
        e.CustomerId.Should().Be(customerId);
        e.CustomerEmail.Should().Be(customerEmail);
        e.EntityId.Should().Be(shipmentId);
    }

    [Fact(DisplayName = "ShipmentShippedEvent should allow null Carrier")]
    public void Constructor_ShouldAllowNullCarrier()
    {
        var e = new ShipmentEvent.Lifecycle.Shipped(Guid.NewGuid(), Guid.NewGuid(), "TRACK", null, DateTimeOffset.UtcNow, Guid.NewGuid(), "customer@example.com");

        e.Carrier.Should().BeNull();
    }
}
