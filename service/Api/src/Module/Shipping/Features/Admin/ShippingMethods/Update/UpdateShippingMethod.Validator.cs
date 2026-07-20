using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Validators;

namespace Module.Shipping.Features.Admin.ShippingMethods.Update;

public static partial class UpdateShippingMethod
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyShippingMethodParametersRules();
        }
    }
}