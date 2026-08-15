using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Mappings;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.UnitTests.Shipping.Features.Admin.Shipments.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ShipmentMapping")]
public class ShipmentMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map Shipment to detail response")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var shipment = CreateShipment();

        var response = shipment.MapToDetail<ShipmentDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(shipment.Id);
        response.OrderId.Should().Be(shipment.OrderId);
        response.ShippingMethodId.Should().Be(shipment.ShippingMethodId);
        response.TrackingNumber.Should().Be(shipment.TrackingNumber);
        response.Status.Should().Be(shipment.Status);
        response.ShippedAtUtc.Should().Be(shipment.ShippedAtUtc);
        response.DeliveredAtUtc.Should().Be(shipment.DeliveredAtUtc);
        response.EstimatedDeliveryAtUtc.Should().Be(shipment.EstimatedDeliveryAtUtc);
        response.CreatedAtUtc.Should().Be(shipment.CreatedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map Shipment to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var shipment = CreateShipment();

        var response = shipment.MapToListItem<ShipmentListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(shipment.Id);
        response.OrderId.Should().Be(shipment.OrderId);
        response.ShippingMethodId.Should().Be(shipment.ShippingMethodId);
        response.TrackingNumber.Should().Be(shipment.TrackingNumber);
        response.Status.Should().Be(shipment.Status);
        response.ShippedAtUtc.Should().Be(shipment.ShippedAtUtc);
        response.DeliveredAtUtc.Should().Be(shipment.DeliveredAtUtc);
        response.EstimatedDeliveryAtUtc.Should().Be(shipment.EstimatedDeliveryAtUtc);
        response.CreatedAtUtc.Should().Be(shipment.CreatedAtUtc);
    }

    private static Shipment CreateShipment()
    {
        var result = ShipmentMethod.Create(Guid.NewGuid(), Guid.NewGuid());
        result.IsSuccess.Should().BeTrue();
        var shipment = result.Value;
        shipment.TrackingNumber = "TRK-123456";
        shipment.Status = ShipmentStatus.Shipped;
        shipment.ShippedAtUtc = DateTimeOffset.UtcNow.AddHours(-2);
        shipment.DeliveredAtUtc = null;
        shipment.EstimatedDeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(2);
        return shipment;
    }
}
