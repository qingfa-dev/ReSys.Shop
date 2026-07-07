using FluentValidation;
using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Update;

public static partial class UpdatePaymentMethod
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            When(x => x.Request.Name is not null, () =>
            {
                RuleFor(x => x.Request.Name).ApplyNameRules();
            });

            When(x => x.Request.Code is not null, () =>
            {
                RuleFor(x => x.Request.Code).ApplyCodeRules();
            });

            When(x => x.Request.ProviderType is not null, () =>
            {
                RuleFor(x => x.Request.ProviderType).ApplyProviderTypeRules();
            });
        }
    }
}
