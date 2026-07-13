using Module.Payment.Features.Admin.Payments.Shared.Models;

namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public record Response : PaymentDetailResponse
    {
        public decimal CapturedAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
