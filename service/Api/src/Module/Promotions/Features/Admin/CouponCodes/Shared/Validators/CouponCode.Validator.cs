using FluentValidation;
using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Shared.Models;

namespace Module.Promotions.Features.Admin.CouponCodes.Shared.Validators;

/// <summary>Shared validators for coupon code models.</summary>
public static class CouponCodeValidator
{
    public sealed class CouponCodeParametersValidator : AbstractValidator<CouponCodeParameters>
    {
        public CouponCodeParametersValidator()
        {
            RuleFor(x => x.Code).ApplyCodeRules();
        }
    }

    public static IRuleBuilderOptions<T, CouponCodeParameters> ApplyCouponCodeParametersRules<T>(
        this IRuleBuilder<T, CouponCodeParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new CouponCodeParametersValidator());
    }
}
