using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

/// <summary>Handles retrieval of active stock reservations for the current cart.</summary>
public static partial class GetCartReservations
{
    public sealed record Query(string CartToken) : IQuery<List<Response>>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Executes the get cart reservations query.</summary>
        /// <param name="request">The query containing the cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active reservations with remaining TTL.</returns>
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var reservations = await dbContext.Set<StockReservation>()
                .Where(r => r.CartToken == request.CartToken
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > now)
                .ToListAsync(cancellationToken);

            return reservations.Select(r => new Response
            {
                Id = r.Id,
                VariantId = r.VariantId,
                Quantity = r.Quantity,
                ExpiresAtUtc = r.ExpiresAtUtc!.Value,
                State = r.State.ToString(),
                RemainingSeconds = (int)(r.ExpiresAtUtc!.Value - now).TotalSeconds
            }).ToList();
        }
    }
}
