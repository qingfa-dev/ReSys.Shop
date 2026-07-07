using FluentValidation;

namespace Module.Promotions.Features.Admin.CouponCodes.Delete;

public static partial class DeleteCouponCode
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("CouponCode.Id.Required")
                .WithMessage("Coupon code ID is required.");
        }
    }
}
