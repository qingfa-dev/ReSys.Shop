using Module.Payment.Domain.Payments;

namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public decimal CapturedAmount { get; init; }
        public PaymentState State { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
