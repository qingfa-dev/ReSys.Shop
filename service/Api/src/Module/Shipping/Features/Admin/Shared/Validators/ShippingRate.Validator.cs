using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.Shared.Validators;

public static class ShippingRateValidator
{
    public sealed class ShippingRateParametersValidator : AbstractValidator<ShippingRateParameters>
    {
        public ShippingRateParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Cost).ApplyCostRules();
            RuleFor(x => x.DeliveryRange).ApplyDeliveryRangeRules();
        }
    }

    public static IRuleBuilderOptions<T, ShippingRateParameters> ApplyShippingRateParametersRules<T>(
        this IRuleBuilder<T, ShippingRateParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new ShippingRateParametersValidator());
    }
}