using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

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
        => MapToDetailCore<T>(entity, itemLookup: null);

    /// <summary>Maps an Order entity to a detail response, enriching line items with product references (id, name, primary image).</summary>
    public static T MapToDetailWithLookup<T>(this Order entity, IReadOnlyDictionary<Guid, CartItemLookup> itemLookup)
        where T : OrderDetailResponse, new()
        => MapToDetailCore<T>(entity, itemLookup);

    private static T MapToDetailCore<T>(Order entity, IReadOnlyDictionary<Guid, CartItemLookup>? itemLookup)
        where T : OrderDetailResponse, new()
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
            ShippingAdjustment = entity.Adjustments.FirstOrDefault(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping) is { } sa
                ? new ShippingAdjustmentSummary { Id = sa.Id, Label = sa.Label, Amount = sa.Amount, ShippingMethodId = sa.SourceId }
                : null,
            ShippingCalculation = entity.ShippingMethodId.HasValue
                ? new ShippingCalculationSummary
                {
                    TotalWeight = entity.TotalWeight,
                    ShippingRateId = entity.ShippingRateId,
                    Cost = entity.ShipmentTotal,
                    IsFreeShipping = entity.IsFreeShipping,
                }
                : null,
            Adjustments = entity.Adjustments.Select(a => new AdjustmentSummary
            {
                Id = a.Id,
                Label = a.Label,
                Amount = a.Amount,
                SourceType = a.SourceType,
                ShippingMethodId = a.SourceType == AdjustmentConstant.SourceTypes.Shipping ? a.SourceId : (Guid?)null,
            }).ToList(),
            Total = entity.Total,
            PaymentTotal = entity.PaymentTotal,
            OutstandingBalance = entity.OutstandingBalance,
            PaymentState = entity.PaymentState,
            FulfillmentState = entity.FulfillmentState,
            UserId = entity.UserId,
            ItemCount = entity.ItemCount,
            ApprovedById = entity.ApprovedById,
            ApprovedAtUtc = entity.ApprovedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            CanceledAtUtc = entity.CanceledAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            LineItems = entity.LineItems
                .OrderBy(li => li.CreatedAtUtc)
                .Select(li => itemLookup is null
                    ? li.MapToLineItemResponse<LineItemResponse>()
                    : li.MapToLineItemResponse<LineItemResponse>(itemLookup.GetValueOrDefault(li.VariantId)))
                .ToList(),
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
            FulfillmentState = entity.FulfillmentState,
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

    /// <summary>Maps a LineItem to a response DTO, enriching it with product references from the lookup.</summary>
    public static T MapToLineItemResponse<T>(this LineItem entity, CartItemLookup? lookup) where T : LineItemResponse, new()
    {
        var response = entity.MapToLineItemResponse<T>();
        if (lookup is null)
            return response;
        return response with
        {
            ProductId = lookup.ProductId,
            ProductName = lookup.ProductName,
            ProductImageUrl = lookup.ProductImageUrl,
        };
    }
}