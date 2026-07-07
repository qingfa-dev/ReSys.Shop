using FluentValidation;
using Module.Shipping.Features.Admin.Shipments.Shared.Validators;

namespace Module.Shipping.Features.Admin.Shipments.Create;

public static partial class CreateShipment
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyShipmentParametersRules();
        }
    }
}
