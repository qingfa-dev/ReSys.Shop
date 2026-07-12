using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

// Boundary: Features → Models — cart model mapping.
/// <summary>Provides mapping methods from Order domain entities to cart response models.</summary>
public static partial class CartMapping
{
    /// <summary>Maps a domain Order (cart) to a detail response.</summary>
    public static T MapToDetail<T>(this Order entity) where T : CartDetailResponse, new()
    {
        // Map: Project order fields onto response DTO.
        return new T
        {
            Id = entity.Id,
            VariantId = default,
            Quantity = 0,
            Notes = null,
        };
    }
}
