using Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Payment.Features.Admin.PaymentMethods.Get.ById;

public static partial class GetPaymentMethodById
{
    public record Response : PaymentMethodDetailResponse;
}