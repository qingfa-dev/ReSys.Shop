using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Mappings;

public static partial class ShipmentMapping
{
    public static T MapToDetail<T>(this Shipment entity) where T : ShipmentDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number ?? string.Empty,
            Tracking = entity.Tracking,
            Cost = entity.Cost,
            DiscountedCost = entity.DiscountedCost,
            FinalPrice = entity.FinalPrice,
            ItemCost = entity.ItemCost,
            TaxTotal = entity.TaxTotal,
            PromoTotal = entity.PromoTotal,
            OrderId = entity.OrderId,
            StockLocationId = entity.StockLocationId,
            ShippingMethodId = entity.ShippingMethodId,
            AddressId = entity.AddressId,
            ShippedAtUtc = entity.ShippedAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    public static T MapToListItem<T>(this Shipment entity) where T : ShipmentListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number ?? string.Empty,
            Tracking = entity.Tracking,
            Cost = entity.Cost,
            DiscountedCost = entity.DiscountedCost,
            FinalPrice = entity.FinalPrice,
            ItemCost = entity.ItemCost,
            TaxTotal = entity.TaxTotal,
            PromoTotal = entity.PromoTotal,
            OrderId = entity.OrderId,
            StockLocationId = entity.StockLocationId,
            ShippingMethodId = entity.ShippingMethodId,
            AddressId = entity.AddressId,
            ShippedAtUtc = entity.ShippedAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
