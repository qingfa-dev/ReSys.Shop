using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

// Boundary: Features → Domain — maps Order entities to cart response DTOs
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

    public static CartItem MapToCartItem(this LineItem lineItem, string variantName, string sku)
    {
        return new CartItem
        {
            Id = lineItem.Id,
            VariantId = lineItem.VariantId,
            VariantName = variantName,
            Sku = sku,
            Quantity = lineItem.Quantity,
            Price = lineItem.Price,
            Total = lineItem.Total,
        };
    }

    public static T MapToDetailWithItems<T>(this Order entity, Dictionary<Guid, string> variantNames)
        where T : CartDetailResponse, new()
    {
        var result = entity.MapToDetail<T>();
        result = result with
        {
            Items = entity.LineItems.Select(li =>
            {
                variantNames.TryGetValue(li.VariantId, out var name);
                return li.MapToCartItem(name ?? "", name ?? "");
            }).ToList()
        };
        return result;
    }
}