using Module.Billing.Features.Admin.Shared.Models;

namespace Module.Billing.Features.Admin.Payments.Get.Paged;

public static partial class GetPagedPayments
{
    public record Response : PaymentListItemResponse;
}