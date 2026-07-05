using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockReservations.Get.ById;

/// <summary>Handles retrieval of a stock reservation by identifier.</summary>
public static partial class GetStockReservationById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get stock reservation by id query.</summary>
        /// <param name="query">The query containing the reservation identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the reservation details.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null

            // Query: Retrieve the reservation by identifier.
            var reservation = await dbContext.Set<StockReservation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

            // Check: Verify the reservation exists.
            if (reservation is null)
                return StockReservationResult.Errors.NotFound(query.Id);

            // Map: Return the reservation as response.
            return reservation.MapToDetail<Response>();
        }
    }
}
