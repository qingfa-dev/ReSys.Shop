using Module.Billing.Features.Admin.Shared.Models;

namespace Module.Billing.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    public record Response : PaymentMethodDetailResponse;
}