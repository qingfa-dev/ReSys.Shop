using FluentValidation;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Shared.Models;

namespace Module.Shipping.Features.Admin.MethodRates.Shared.Validators;

public static class MethodRateValidator
{
    public sealed class MethodRateParametersValidator : AbstractValidator<MethodRateParameters>
    {
        public MethodRateParametersValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(ShippingRateResult.Errors.NameRequired.Code)
                .WithMessage(ShippingRateResult.Errors.NameRequired.Description)
                .MaximumLength(ShippingRateConstant.Constraints.MaxNameLength)
                .WithErrorCode(ShippingRateResult.Errors.NameTooLong.Code)
                .WithMessage(ShippingRateResult.Errors.NameTooLong.Description);

            RuleFor(x => x.Cost)
                .GreaterThan(0)
                .WithErrorCode(ShippingRateResult.Errors.CostRequired.Code)
                .WithMessage(ShippingRateResult.Errors.CostRequired.Description);
        }
    }

    public static IRuleBuilderOptions<T, MethodRateParameters> ApplyMethodRateParametersRules<T>(
        this IRuleBuilder<T, MethodRateParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new MethodRateParametersValidator());
    }

    public static IRuleBuilderOptions<T, MethodRateParameters> ApplyMethodRateWeightRules<T>(
        this IRuleBuilder<T, MethodRateParameters> ruleBuilder)
    {
        return ruleBuilder
            .ChildRules(weight =>
            {
                weight.RuleFor(x => x.MinWeight)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.MinWeight.HasValue)
                    .WithErrorCode(ShippingRateResult.Errors.WeightNegative.Code)
                    .WithMessage(ShippingRateResult.Errors.WeightNegative.Description);

                weight.RuleFor(x => x.MaxWeight)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.MaxWeight.HasValue)
                    .WithErrorCode(ShippingRateResult.Errors.WeightNegative.Code)
                    .WithMessage(ShippingRateResult.Errors.WeightNegative.Description);

                weight.RuleFor(x => x.MinWeight)
                    .LessThanOrEqualTo(x => x.MaxWeight!.Value)
                    .When(x => x.MinWeight.HasValue && x.MaxWeight.HasValue)
                    .WithErrorCode(ShippingRateResult.Errors.MinWeightExceedsMaxWeight.Code)
                    .WithMessage(ShippingRateResult.Errors.MinWeightExceedsMaxWeight.Description);
            });
    }
}
