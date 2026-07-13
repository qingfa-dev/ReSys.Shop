using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Create;

public static partial class CreateShippingMethod
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).ApplyNameRules();
            RuleFor(x => x.Request.Code).ApplyCodeRules();
            RuleFor(x => x.Request.CalculatorType).ApplyCalculatorTypeRules();
        }
    }
}