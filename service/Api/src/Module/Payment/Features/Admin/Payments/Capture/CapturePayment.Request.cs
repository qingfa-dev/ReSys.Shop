namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    // EXCEPTION: feature-specific capture request — no domain entity base
    public record Request
    {
        public decimal? Amount { get; init; }
    }
}
