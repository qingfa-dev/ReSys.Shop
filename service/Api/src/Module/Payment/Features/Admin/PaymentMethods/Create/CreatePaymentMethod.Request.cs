using Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Payment.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    public record Request : PaymentMethodRequest;
}