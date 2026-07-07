namespace Module.Shipping.Features.Storefront.Shipping.Calculate;

public static partial class CalculateShipping
{
    public sealed class Response
    {
        public Guid ShippingMethodId { get; init; }
        public string MethodName { get; init; } = string.Empty;
        public decimal Cost { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool IsFreeShipping { get; init; }
    }
}
