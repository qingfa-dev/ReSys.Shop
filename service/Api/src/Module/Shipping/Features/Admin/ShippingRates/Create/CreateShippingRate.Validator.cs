using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingRates.Create;

public static partial class CreateShippingRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).ApplyNameRules();
            RuleFor(x => x.Request.Cost).ApplyCostRules();
            RuleFor(x => x.Request.DeliveryRange).ApplyDeliveryRangeRules();
        }
    }
}