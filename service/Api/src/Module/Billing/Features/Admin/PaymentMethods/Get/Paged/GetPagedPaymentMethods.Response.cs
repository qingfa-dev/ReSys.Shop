using Module.Billing.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Billing.Features.Admin.PaymentMethods.Get.Paged;

public static partial class GetPagedPaymentMethods
{
    public record Response : PaymentMethodListItemResponse;
}