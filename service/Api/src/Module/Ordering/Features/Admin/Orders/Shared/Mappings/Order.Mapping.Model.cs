using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Mappings;

public static partial class OrderMapping
{
    // Map: Domain entity -> Detail response
    public static T MapToDetail<T>(this Order entity) where T : OrderDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number,
            Status = entity.Status,
            CheckoutState = entity.CheckoutState,
            Currency = entity.Currency,
            Email = entity.Email,
            SpecialInstructions = entity.SpecialInstructions,
            BillAddressId = entity.BillAddressId,
            ShipAddressId = entity.ShipAddressId,
            ShippingMethodId = entity.ShippingMethodId,
            ItemTotal = entity.ItemTotal,
            AdjustmentTotal = entity.AdjustmentTotal,
            ShipmentTotal = entity.ShipmentTotal,
            Total = entity.Total,
            PaymentTotal = entity.PaymentTotal,
            OutstandingBalance = entity.OutstandingBalance,
            PaymentState = entity.PaymentState,
            ShipmentState = entity.ShipmentState,
            UserId = entity.UserId,
            StoreId = entity.StoreId,
            CompletedAtUtc = entity.CompletedAtUtc,
            CanceledAtUtc = entity.CanceledAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }

    // Map: Domain entity -> List item response
    public static T MapToListItem<T>(this Order entity) where T : OrderListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number,
            Status = entity.Status,
            Currency = entity.Currency,
            Total = entity.Total,
            PaymentTotal = entity.PaymentTotal,
            PaymentState = entity.PaymentState,
            ShipmentState = entity.ShipmentState,
            BillAddressId = entity.BillAddressId,
            ShipAddressId = entity.ShipAddressId,
            Email = entity.Email,
            CreatedAtUtc = entity.CreatedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
        };
    }
}
