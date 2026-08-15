using Module.Billing.Features.Admin.Payments.Shared.Models;

namespace Module.Billing.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public sealed record Request : PaymentActionParameters;
}
