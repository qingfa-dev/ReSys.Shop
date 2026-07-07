namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    public sealed class Response
    {
        public Guid Id { get; init; }
        public Guid ShippingMethodId { get; init; }
        public string Name { get; init; } = null!;
        public decimal Cost { get; init; }
        public decimal FinalPrice { get; init; }
        public string? DeliveryRange { get; init; }
        public decimal? MinWeight { get; init; }
        public decimal? MaxWeight { get; init; }
        public decimal? FreeShippingThreshold { get; init; }
    }
}
