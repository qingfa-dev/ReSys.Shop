using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

public static partial class CartMapping
{
    public static T MapToDetail<T>(this Order entity) where T : CartDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            ItemTotal = entity.ItemTotal,
            Total = entity.Total,
            Currency = entity.Currency,
            ItemCount = entity.ItemCount,
            CheckoutState = entity.CheckoutState.ToString(),
        };
    }
}
