using FluentValidation;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Validators;

public static class ShippingMethodValidator
{
    public sealed class ShippingMethodParametersValidator : AbstractValidator<ShippingMethodParameters>
    {
        public ShippingMethodParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.CalculatorType)
                .NotEmpty()
                .WithErrorCode(ShippingMethodResult.Errors.CalculatorRequired.Code)
                .WithMessage(ShippingMethodResult.Errors.CalculatorRequired.Description);
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
