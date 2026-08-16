using Module.Shipping.Features.Admin.Shared.Validators;

namespace Module.Shipping.Features.Admin.ShippingMethods.Create;

public static partial class CreateShippingMethod
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