using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Validators;

public static class ShippingMethodValidator
{
    public sealed class ShippingMethodParametersValidator : AbstractValidator<Models.ShippingMethodParameters>
    {
        public ShippingMethodParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Code).ApplyCodeRules();
            RuleFor(x => x.CalculatorType).ApplyCalculatorTypeRules();
        }
    }

    public static IRuleBuilderOptions<T, Models.ShippingMethodParameters> ApplyShippingMethodParametersRules<T>(
        this IRuleBuilder<T, Models.ShippingMethodParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new ShippingMethodParametersValidator());
    }
}