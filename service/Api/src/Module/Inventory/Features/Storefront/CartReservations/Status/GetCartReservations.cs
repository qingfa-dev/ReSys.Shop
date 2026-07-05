using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

/// <summary>Handles retrieval of active stock reservations for the current cart.</summary>
public static partial class GetCartReservations
{
    public sealed record Query(string CartToken) : IQuery<List<Response>>;

    public sealed class QueryHandler(IStockChecker stockChecker)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Executes the get cart reservations query.</summary>
        /// <param name="request">The query containing the cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active reservations with remaining TTL.</returns>
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve active cart reservations via StockChecker
            var reservations = await stockChecker.GetReservationsForCartAsync(request.CartToken, cancellationToken);

            // Map: Return reservations with remaining seconds
            return reservations.Select(r => new Response
            {
                Id = r.Reservation.Id,
                VariantId = r.Reservation.VariantId,
                Quantity = r.Reservation.Quantity,
                ExpiresAtUtc = r.Reservation.ExpiresAtUtc!.Value,
                State = r.Reservation.State.ToString(),
                RemainingSeconds = r.RemainingSeconds
            }).ToList();
        }
    }
}
