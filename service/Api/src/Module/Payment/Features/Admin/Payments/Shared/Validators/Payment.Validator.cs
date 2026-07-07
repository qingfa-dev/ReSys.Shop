namespace Module.Payment.Features.Admin.Payments.Shared.Validators;

public static partial class PaymentValidator
{
    public sealed class PaymentParametersValidator : AbstractValidator<Models.PaymentParameters>
    {
        public PaymentParametersValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.PaymentMethodId).NotEmpty();
        }
    }

    public static IRuleBuilderOptions<T, Models.PaymentParameters> ApplyPaymentParametersRules<T>(
        this IRuleBuilder<T, Models.PaymentParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new PaymentParametersValidator());
    }
}
