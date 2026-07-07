using Module.Payment.Domain.Payments;

namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public decimal RefundedAmount { get; init; }
        public PaymentState State { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
