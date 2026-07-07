using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Update;

public static partial class UpdateShippingMethod
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            When(x => x.Request.Name is not null, () =>
            {
                RuleFor(x => x.Request.Name).ApplyNameRules();
            });

            When(x => x.Request.Code is not null, () =>
            {
                RuleFor(x => x.Request.Code).ApplyCodeRules();
            });

            When(x => x.Request.CalculatorType is not null, () =>
            {
                RuleFor(x => x.Request.CalculatorType).ApplyCalculatorTypeRules();
            });
        }
    }
}
