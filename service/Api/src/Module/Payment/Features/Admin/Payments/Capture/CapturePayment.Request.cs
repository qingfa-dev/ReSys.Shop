namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public record Request
    {
        public decimal? Amount { get; init; }
    }
}
