using Module.Shipping.Features.Admin.MethodRates.Shared.Models;

namespace Module.Shipping.Features.Admin.MethodRates.Update;
public static partial class UpdateMethodRate
{
    public class Request : MethodRateRequest
    {
        public new string? Name { get; init; }
        public new decimal? Cost { get; init; }
        public new string? DeliveryRange { get; init; }
        public new decimal? MinWeight { get; init; }
        public new decimal? MaxWeight { get; init; }
        public new decimal? FreeShippingThreshold { get; init; }
    }
}
