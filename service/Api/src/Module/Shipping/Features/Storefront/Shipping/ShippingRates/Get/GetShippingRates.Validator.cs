namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class GetShippingRates
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Parameters.PageSize).GreaterThanOrEqualTo(1);
        }
    }
}