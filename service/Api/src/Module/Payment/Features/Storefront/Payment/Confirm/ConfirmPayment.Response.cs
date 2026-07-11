using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = null!;
        public decimal Amount { get; init; }
        public PaymentRecordState State { get; init; }
        public string Message { get; init; } = null!;
    }
}
