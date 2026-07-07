namespace Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

/// <summary>Cart domain mapping — cart creation uses OrderExtensions.Create directly.</summary>
public static partial class CartMapping
{
    // No MapToDomain overload needed — AddToCart creates LineItems, not Orders.
    // Cart creation via CreateOrderFromCart uses OrderExtensions.Create.
}
