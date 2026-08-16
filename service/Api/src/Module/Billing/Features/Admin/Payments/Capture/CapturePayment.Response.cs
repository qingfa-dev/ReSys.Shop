using Module.Billing.Features.Admin.Shared.Models;

namespace Module.Billing.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public record Response : PaymentDetailResponse
    {
        public decimal CapturedAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
