using BuildingBlocks.Querying.Helpers;
using FluentValidation;

namespace Module.Shipping.Features.Admin.Shipments.Get.Paged;

public static partial class GetPagedShipments
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
