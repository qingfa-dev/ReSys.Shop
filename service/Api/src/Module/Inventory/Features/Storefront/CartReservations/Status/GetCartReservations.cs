using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

/// <summary>Lists active, non-expired stock reservations for a given cart token with remaining TTL.</summary>
public static partial class GetCartReservations
{
    public sealed record Query(string CartToken) : IQuery<List<Response>>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, List<Response>>
    {
        /// <summary>Fetches all active reservations for the cart and computes remaining seconds before expiry.</summary>
        /// <param name="request">The query containing the cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active reservations with remaining TTL.</returns>
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
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
