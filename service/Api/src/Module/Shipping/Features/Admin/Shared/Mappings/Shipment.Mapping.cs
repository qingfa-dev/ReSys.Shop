using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.Shared.Mappings;

public static partial class ShipmentMapping
{
    public static T MapToListItem<T>(this Shipment entity) where T : ShipmentListItemResponse, new()
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
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    public static T MapToDetail<T>(this Shipment entity) where T : ShipmentDetailResponse, new()
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
