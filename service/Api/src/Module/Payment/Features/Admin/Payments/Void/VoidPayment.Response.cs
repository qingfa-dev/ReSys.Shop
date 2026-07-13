using Module.Payment.Features.Admin.Payments.Shared.Models;

namespace Module.Payment.Features.Admin.Payments.Void;

public static partial class VoidPayment
{
    public record Response : PaymentDetailResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
