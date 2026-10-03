using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Amount).ApplyAmountRules();
        }
    }
}