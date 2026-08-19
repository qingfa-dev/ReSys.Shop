using Module.Shipping.Features.Admin.Shared.Validators;

namespace Module.Shipping.Features.Admin.ShippingRates.Update;

public static partial class UpdateShippingRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m=>m.Request)
                .ApplyShippingRateParametersRules();
        }
    }
}