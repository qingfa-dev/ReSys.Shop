using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Shared.Validators;

/// <summary>Shared validators for payment method parameters and requests.</summary>
public static class PaymentMethodValidator
{
    /// <summary>Validates PaymentMethodParameters using domain validation rules.</summary>
    public sealed class PaymentMethodParametersValidator : AbstractValidator<Models.PaymentMethodParameters>
    {
        public PaymentMethodParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Code).ApplyCodeRules();
            RuleFor(x => x.ProviderKey).ApplyProviderKeyRules();
        }
    }

    /// <summary>Extension method to apply payment method parameters validation.</summary>
    public static IRuleBuilderOptions<T, Models.PaymentMethodParameters> ApplyPaymentMethodParametersRules<T>(
        this IRuleBuilder<T, Models.PaymentMethodParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new PaymentMethodParametersValidator());
    }
}