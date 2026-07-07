using BuildingBlocks.Querying.Helpers;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Get.Paged;

public static partial class GetPagedOrders
{
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Parameters).NotNull();

            When(x => x.Parameters is not null, () =>
            {
                RuleFor(x => x.Parameters.PageIndex)
                    .ApplyPageValidation()
                    .When(x => x.Parameters.PageIndex.HasValue);

                RuleFor(x => x.Parameters.PageSize)
                    .ApplyPageSizeValidation()
                    .When(x => x.Parameters.PageSize.HasValue);

                RuleFor(x => x.Parameters.Search)
                    .ApplySearchValidation();

                RuleFor(x => x.Parameters.SearchField)
                    .ApplySearchFieldsValidation(OrderConstant.Query.AllowedSearchFields);

                RuleFor(x => x.Parameters.OrderBy)
                    .ApplySortValidation(OrderConstant.Query.AllowedSortFields);

                RuleFor(x => x.Parameters.Filter)
                    .ApplyFilterValidation(OrderConstant.Query.AllowedFilterFields, checkDangerous: true);
            });
        }
    }
}
