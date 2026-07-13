namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public sealed class Request
    {
        public Guid OrderId { get; init; }
        public Guid ShippingMethodId { get; init; }
    }
}