using Module.Shipping.Features.Admin.Shared.Models;

using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Admin.Shared.Validators;

public static class ShipmentValidator
{
    public sealed class ShipmentParametersValidator : AbstractValidator<ShipmentParameters>
    {
        public ShipmentParametersValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode(ShipmentResult.Errors.OrderIdRequired.Code)
                .WithMessage(ShipmentResult.Errors.OrderIdRequired.Message);

            RuleFor(x => x.ShippingMethodId)
                .NotEmpty()
                .WithErrorCode(ShipmentResult.Errors.ShippingMethodIdRequired.Code)
                .WithMessage(ShipmentResult.Errors.ShippingMethodIdRequired.Message);
        }
    }

    public static IRuleBuilderOptions<T, ShipmentParameters> ApplyShipmentParametersRules<T>(
        this IRuleBuilder<T, ShipmentParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new ShipmentParametersValidator());
    }
}