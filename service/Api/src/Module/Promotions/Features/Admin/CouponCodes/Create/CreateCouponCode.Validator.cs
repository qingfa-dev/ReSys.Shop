using FluentValidation;
using Module.Promotions.Features.Admin.CouponCodes.Shared.Validators;

namespace Module.Promotions.Features.Admin.CouponCodes.Create;

public static partial class CreateCouponCode
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyCouponCodeParametersRules();
        }
    }
}
