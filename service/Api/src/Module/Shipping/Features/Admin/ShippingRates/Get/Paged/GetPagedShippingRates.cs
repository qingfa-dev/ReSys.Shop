using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Get.Paged;

public static partial class GetPagedShippingRates
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

            var pagedResult = await dbContext.Set<ShippingRate>()
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, r => r.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
