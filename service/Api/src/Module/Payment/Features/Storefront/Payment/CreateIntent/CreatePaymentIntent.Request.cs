using Module.Payment.Features.Admin.Payments.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public class Request : PaymentRequest
    {
        public string? ReturnUrl { get; init; }
    }
}
