namespace Module.Billing.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public sealed record Request
    {
        public decimal? Amount { get; init; }
    }
}
