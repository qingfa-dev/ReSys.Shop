using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockLocations.GetById;

public static partial class GetStockLocationById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>Handler for getting a stock location by ID.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Gets a stock location by ID.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the stock location by identifier without tracking
            var entity = await dbContext.Set<StockLocation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check: Return not-found if no location matches
            if (entity is null)
                return StockLocationResult.Errors.NotFound;

            // Transform: Map domain entity to response DTO
            return entity.MapToDetail<Response>();
        }
    }
}