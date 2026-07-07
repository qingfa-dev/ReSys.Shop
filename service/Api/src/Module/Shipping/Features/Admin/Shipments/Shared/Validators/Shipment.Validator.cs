using FluentValidation;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Validators;

public static class ShipmentValidator
{
    public sealed class ShipmentParametersValidator : AbstractValidator<ShipmentParameters>
    {
        public ShipmentParametersValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode(ShipmentResult.Errors.OrderIdRequired.Code)
                .WithMessage(ShipmentResult.Errors.OrderIdRequired.Description);

            RuleFor(x => x.StockLocationId)
                .NotEmpty()
                .WithErrorCode(ShipmentResult.Errors.StockLocationIdRequired.Code)
                .WithMessage(ShipmentResult.Errors.StockLocationIdRequired.Description);
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
