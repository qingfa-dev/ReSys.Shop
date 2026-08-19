using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.Shared.Validators;

public static class ShippingMethodValidator
{
    public sealed class ShippingMethodParametersValidator : AbstractValidator<ShippingMethodParameters>
    {
        public ShippingMethodParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Code).ApplyCodeRules();
            RuleFor(x => x.CalculatorType).ApplyCalculatorTypeRules();
            RuleFor(x => x.Presentation).ApplyPresentationRules();
        }
    }

    public static IRuleBuilderOptions<T, ShippingMethodParameters> ApplyShippingMethodParametersRules<T>(
        this IRuleBuilder<T, ShippingMethodParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new ShippingMethodParametersValidator());
    }
}