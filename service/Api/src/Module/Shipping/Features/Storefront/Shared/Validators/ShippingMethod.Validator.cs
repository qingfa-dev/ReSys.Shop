using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Storefront.Shared.Validators;

public static partial class ShippingMethodValidator
{
    public sealed class ShippingMethodParametersValidator : AbstractValidator<Models.ShippingMethodParameters>
    {
        public ShippingMethodParametersValidator()
        {
            RuleFor(x => x.MethodName)
                .NotEmpty()
                .WithErrorCode(ShippingMethodResult.Errors.NameRequired.Code)
                .WithMessage(ShippingMethodResult.Errors.NameRequired.Description)
                .MaximumLength(ShippingMethodConstant.Constraints.MaxNameLength)
                .WithErrorCode(ShippingMethodResult.Errors.NameTooLong.Code)
                .WithMessage(ShippingMethodResult.Errors.NameTooLong.Description);

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(ShippingMethodResult.Errors.CalculatorRequired.Code)
                .WithMessage("Cost must be zero or greater.");
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
