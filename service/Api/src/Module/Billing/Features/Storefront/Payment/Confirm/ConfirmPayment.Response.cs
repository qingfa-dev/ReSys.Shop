using Module.Billing.Features.Storefront.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public record Response : StorePaymentDetailResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
