using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.Paged;

public static partial class GetPagedShippingMethods
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .OrderBy(m => m.Position)
                .ThenBy(m => m.Name)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, m => m.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
