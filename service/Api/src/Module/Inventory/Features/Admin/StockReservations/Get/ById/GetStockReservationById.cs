using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockReservations.Get.ById;

/// <summary>Gets a single stock reservation record by its unique identifier.</summary>
public static partial class GetStockReservationById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Fetches the reservation with no-tracking for read-only access.</summary>
        /// <param name="query">The query containing the reservation identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the reservation details.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null

            // Load: Retrieve the reservation by identifier.
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