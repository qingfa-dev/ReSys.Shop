namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PaymentMethodId).NotEmpty();
        }
    }
}
