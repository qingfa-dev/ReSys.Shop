using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.GetById;

public static partial class GetStockLocationById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<StockLocation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity is null)
                return StockLocationResult.Errors.NotFound;

            return entity.MapToDetail<Response>();
        }
    }
}
