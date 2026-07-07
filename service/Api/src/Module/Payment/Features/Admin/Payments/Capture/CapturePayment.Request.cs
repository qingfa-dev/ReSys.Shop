namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public class Request
    {
        public decimal? Amount { get; init; }
    }
}
