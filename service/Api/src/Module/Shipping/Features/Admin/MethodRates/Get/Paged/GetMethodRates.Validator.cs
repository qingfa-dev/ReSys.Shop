using BuildingBlocks.Querying.Helpers;
using FluentValidation;

namespace Module.Shipping.Features.Admin.MethodRates.Get.Paged;

public static partial class GetMethodRates
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.MethodId).NotEmpty();

            RuleFor(x => x.Parameters.PageIndex).ApplyPageValidation();
            RuleFor(x => x.Parameters.PageSize).ApplyPageSizeValidation();
        }
    }
}
