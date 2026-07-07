using FluentValidation;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.ById;

public static partial class GetShippingMethodById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(ShippingMethodResult.Errors.NotFound.Code)
                .WithMessage("Shipping method ID is required.");
        }
    }
}
