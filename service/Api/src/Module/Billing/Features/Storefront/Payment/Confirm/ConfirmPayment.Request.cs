using Module.Billing.Features.Storefront.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record Request : PaymentConfirmationParameters;
}
