using Module.Billing.Features.Admin.Payments.Shared.Models;

namespace Module.Billing.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public sealed record Request : PaymentRequest
    {
        public string? Reason { get; init; }
    }
}
