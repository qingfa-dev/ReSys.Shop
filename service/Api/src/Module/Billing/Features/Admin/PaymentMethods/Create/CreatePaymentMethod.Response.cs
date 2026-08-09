using Module.Billing.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Billing.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    public record Response : PaymentMethodDetailResponse;
}