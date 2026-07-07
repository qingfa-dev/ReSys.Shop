using FluentValidation;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Deactivate;

public static partial class DeactivateShippingMethod
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
