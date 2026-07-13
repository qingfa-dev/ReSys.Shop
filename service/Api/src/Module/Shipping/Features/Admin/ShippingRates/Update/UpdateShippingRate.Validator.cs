using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingRates.Update;

public static partial class UpdateShippingRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            When(x => x.Request.Name is not null, () =>
            {
                RuleFor(x => x.Request.Name!).ApplyNameRules();
            });

            When(x => x.Request.Cost is not null, () =>
            {
                RuleFor(x => x.Request.Cost!.Value).ApplyCostRules();
            });

            When(x => x.Request.DeliveryRange is not null, () =>
            {
                RuleFor(x => x.Request.DeliveryRange!).ApplyDeliveryRangeRules();
            });
        }
    }
}