namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public record Request
    {
        public decimal Amount { get; init; }
        public string? Reason { get; init; }
    }
}
