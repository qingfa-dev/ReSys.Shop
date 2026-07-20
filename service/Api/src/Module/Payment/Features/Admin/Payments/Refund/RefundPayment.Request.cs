using Module.Payment.Features.Admin.Payments.Shared.Models;

namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public sealed record Request : PaymentRequest
    {
        public string? Reason { get; init; }
    }
}
