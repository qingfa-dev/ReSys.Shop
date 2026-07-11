using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public PaymentRecordState State { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
