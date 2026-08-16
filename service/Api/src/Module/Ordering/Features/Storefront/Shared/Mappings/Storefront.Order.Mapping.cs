using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Shared.Models;

namespace Module.Ordering.Features.Storefront.Shared.Mappings;

// Map: Order entity → storefront order list item DTO
public static class OrderStoreMapping
{
    public static T MapToStoreListItem<T>(this Order entity) where T : StorefrontOrderListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number,
            Status = entity.Status,
            Total = entity.Total,
            Currency = entity.Currency,
            ItemCount = entity.ItemCount,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
