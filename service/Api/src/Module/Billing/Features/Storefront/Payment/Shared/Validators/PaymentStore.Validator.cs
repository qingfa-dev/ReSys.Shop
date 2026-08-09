using FluentValidation;

namespace Module.Billing.Features.Storefront.Payment.Shared.Validators;

public static class PaymentParametersValidatorExtensions
{
    public static IRuleBuilderOptions<T, decimal> ApplyAmountRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode("Payment.Amount.Invalid")
            .WithMessage("Amount must be greater than zero.");
    }
}
