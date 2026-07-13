using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Mappings;

/// <summary>Maps Order domain entities to response DTOs for the admin order features.</summary>
// Boundary: Domain → Features — converts persisted entities to wire-format responses
public static partial class OrderMapping
{
    /// <summary>Maps an Order entity to a detail response DTO with all order properties.</summary>
    /// <typeparam name="T">The target response type (must inherit from OrderDetailResponse).</typeparam>
    /// <param name="entity">The Order domain entity to map.</param>
    /// <returns>A new response DTO populated from the entity.</returns>
    // Map: Domain entity -> Detail response (full property transfer)
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
            ItemCount = entity.ItemCount,
            ApprovedById = entity.ApprovedById,
            ApprovedAtUtc = entity.ApprovedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            CanceledAtUtc = entity.CanceledAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }

    /// <summary>Maps an Order entity to a list-item response DTO with summary properties.</summary>
    /// <typeparam name="T">The target response type (must inherit from OrderListItemResponse).</typeparam>
    /// <param name="entity">The Order domain entity to map.</param>
    /// <returns>A new list-item response DTO populated from the entity.</returns>
    // Map: Domain entity -> List item response (summary for grid views)
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

    /// <summary>Maps a LineItem domain entity to a line item response DTO.</summary>
    /// <typeparam name="T">The target response type (must inherit from LineItemResponse).</typeparam>
    /// <param name="entity">The LineItem domain entity to map.</param>
    /// <returns>A new line item response DTO populated from the entity.</returns>
    // Map: Domain entity -> Line item response (full property transfer)
    public static T MapToLineItemResponse<T>(this LineItem entity) where T : LineItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            VariantId = entity.VariantId,
            Quantity = entity.Quantity,
            Price = entity.Price,
            Total = entity.Total,
            AdjustmentTotal = entity.AdjustmentTotal,
            Currency = entity.Currency,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}