namespace Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

// Boundary: Features → Domain — cart domain mapping.
/// <summary>Cart domain mapping — cart creation uses OrderMethod.Create directly.</summary>
public static partial class CartMapping
{
    // No MapToDomain overload needed — AddToCart creates LineItems, not Orders.
    // Cart creation via CreateOrderFromCart uses OrderMethod.Create.
}