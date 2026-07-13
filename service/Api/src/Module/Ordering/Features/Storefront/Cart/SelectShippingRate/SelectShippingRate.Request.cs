namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    public sealed record Request
    {
        public Guid ShippingMethodId { get; init; }
    }
}
