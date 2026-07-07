namespace Module.Shipping.Features.Admin.MethodRates.Get.Paged;
public static partial class GetMethodRates
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Cost { get; init; }
        public decimal FinalPrice { get; init; }
        public Guid ShippingMethodId { get; init; }
        public bool Selected { get; init; }
        public string? DeliveryRange { get; init; }
        public decimal? MinWeight { get; init; }
        public decimal? MaxWeight { get; init; }
        public decimal? FreeShippingThreshold { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
