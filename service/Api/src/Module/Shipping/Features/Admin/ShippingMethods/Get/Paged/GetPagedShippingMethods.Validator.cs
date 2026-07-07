using BuildingBlocks.Querying.Helpers;
using FluentValidation;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.Paged;

public static partial class GetPagedShippingMethods
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
