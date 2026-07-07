namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

public static partial class SelectShippingRate
{
    public class Request
    {
        public Guid ShippingMethodId { get; init; }
    }
}
