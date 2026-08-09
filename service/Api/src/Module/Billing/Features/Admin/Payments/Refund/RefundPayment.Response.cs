using Module.Billing.Features.Admin.Payments.Shared.Models;

namespace Module.Billing.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public record Response : PaymentDetailResponse
    {
        public decimal RefundedAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
