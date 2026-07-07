using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingRates.Shared.Validators;

public static class ShippingRateValidator
{
    public sealed class ShippingRateParametersValidator : AbstractValidator<Models.ShippingRateParameters>
    {
        public ShippingRateParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Cost).ApplyCostRules();
            RuleFor(x => x.DeliveryRange).ApplyDeliveryRangeRules();
        }
    }

    public static IRuleBuilderOptions<T, Models.ShippingRateParameters> ApplyShippingRateParametersRules<T>(
        this IRuleBuilder<T, Models.ShippingRateParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new ShippingRateParametersValidator());
    }
}
