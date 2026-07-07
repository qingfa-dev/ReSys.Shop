namespace Module.Payment.Domain.PaymentCaptureEvents;

public static class PaymentCaptureEventConstant
{
    public static class Constraints
    {
        public const int Precision = 18;
        public const int Scale = 2;
    }

    public static class Query
    {
        public static readonly string[] AllowedFilter =
        [
            nameof(PaymentCaptureEvent.PaymentId)
        ];
    }
}