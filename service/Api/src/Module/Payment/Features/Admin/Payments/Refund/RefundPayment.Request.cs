namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    // EXCEPTION: feature-specific refund request — no domain entity base
    public record Request
    {
        public decimal Amount { get; init; }
        public string? Reason { get; init; }
    }
}
