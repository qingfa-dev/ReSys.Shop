using Module.Billing.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Billing.Features.Admin.PaymentMethods.Get.ById;

public static partial class GetPaymentMethodById
{
    public record Response : PaymentMethodDetailResponse;
}