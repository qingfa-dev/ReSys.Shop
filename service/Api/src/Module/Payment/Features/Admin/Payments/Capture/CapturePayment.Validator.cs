using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Admin.Payments.Capture;

public static partial class CapturePayment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Amount!.Value)
                .ApplyAmountRules()
                .When(x => x.Request.Amount.HasValue);
        }
    }
}
