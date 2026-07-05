using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.GetPaged;

public static partial class GetPagedStockLocations
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var parseAll = parameters.ParseAll();
            if (parseAll.IsFailure)
                return parseAll.Errors;

            var pagedResult = await dbContext.Set<StockLocation>()
                .AsNoTracking()
                .ApplyQuerying(parseAll.Value)
                .ToPagedOrAllAsync(parseAll.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
