using FluentValidation;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Activate;

public static partial class ActivateShippingMethod
{
    public sealed class Validator : AbstractValidator<Command>
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
