using Module.Payment.Features.Admin.Payments.Shared.Models;

namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public record Response : PaymentDetailResponse
    {
        public decimal RefundedAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
