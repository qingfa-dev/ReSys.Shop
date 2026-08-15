using Module.Shipping.Domain.Shipments;

using ShipmentDomain = Module.Shipping.Domain.Shipments.Shipment;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Mappings;

public static partial class ShipmentMapping
{
    public static T MapToListItem<T>(this ShipmentDomain entity) where T : Models.ShipmentListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            ShippingMethodId = entity.ShippingMethodId,
            TrackingNumber = entity.TrackingNumber,
            Status = entity.Status,
            ShippedAtUtc = entity.ShippedAtUtc,
            DeliveredAtUtc = entity.DeliveredAtUtc,
            EstimatedDeliveryAtUtc = entity.EstimatedDeliveryAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public static T MapToDetail<T>(this ShipmentDomain entity) where T : Models.ShipmentDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            ShippingMethodId = entity.ShippingMethodId,
            TrackingNumber = entity.TrackingNumber,
            Status = entity.Status,
            ShippedAtUtc = entity.ShippedAtUtc,
            DeliveredAtUtc = entity.DeliveredAtUtc,
            EstimatedDeliveryAtUtc = entity.EstimatedDeliveryAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
