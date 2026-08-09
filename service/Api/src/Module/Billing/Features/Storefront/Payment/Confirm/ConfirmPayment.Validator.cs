namespace Module.Billing.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PaymentId).NotEmpty();
            // PaymentMethodId is optional — captured for audit, not required for confirm
        }
    }
}