using Module.Payment.Features.Storefront.Payment.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public record Response : StorePaymentDetailResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
