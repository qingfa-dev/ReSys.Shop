using Module.Payment.Features.Storefront.Payment.Shared.Validators;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
