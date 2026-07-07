using FluentValidation;

namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Amount).GreaterThan(0);
        }
    }
}
