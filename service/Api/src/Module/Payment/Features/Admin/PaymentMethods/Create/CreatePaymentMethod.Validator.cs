using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).ApplyNameRules();
            RuleFor(x => x.Request.Code).ApplyCodeRules();
            RuleFor(x => x.Request.ProviderKey).ApplyProviderKeyRules();
        }
    }
}
