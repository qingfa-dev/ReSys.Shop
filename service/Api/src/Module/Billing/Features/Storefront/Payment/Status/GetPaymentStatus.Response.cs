using Module.Billing.Features.Storefront.Payment.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Status;

public static partial class GetPaymentStatus
{
    public record Response : StorePaymentDetailResponse
    {
        public bool IsCompleted { get; init; }
    }
}