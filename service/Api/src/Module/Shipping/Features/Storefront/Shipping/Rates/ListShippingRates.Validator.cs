using BuildingBlocks.Querying.Helpers;
using FluentValidation;

namespace Module.Shipping.Features.Storefront.Shipping.Rates;

public static partial class ListShippingRates
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters.PageIndex).ApplyPageValidation();
            RuleFor(x => x.Parameters.PageSize).ApplyPageSizeValidation();
        }
    }
}
