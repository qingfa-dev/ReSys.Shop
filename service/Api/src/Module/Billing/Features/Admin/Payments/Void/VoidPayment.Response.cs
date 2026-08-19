using Module.Billing.Features.Admin.Shared.Models;

namespace Module.Billing.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    public record Response : PaymentDetailResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
