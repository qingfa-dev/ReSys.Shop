using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

/// <summary>Lists active, non-expired stock reservations for a given cart token with remaining TTL.</summary>
public static partial class GetCartReservations
{
    public sealed record Query(string CartToken, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Fetches active reservations for the cart with DB-side paging and computes remaining seconds before expiry.</summary>
        /// <param name="request">The query containing the cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A page of active reservations with remaining TTL.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var now = DateTimeOffset.UtcNow;
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;

            // Query: Filter active, non-expired reservations for the cart token.
            var query = dbContext.Set<StockReservation>()
                .AsNoTracking()
                .Where(r => r.CartToken == request.CartToken
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > now)
                .OrderBy(r => r.ExpiresAtUtc);

            // Transform: Page in the database, projecting remaining seconds for each reservation.
            return await query.ToPagedOrAllAsync(
                r => new Response
                {
                    Id = r.Id,
                    VariantId = r.VariantId,
                    Quantity = r.Quantity,
                    ExpiresAtUtc = r.ExpiresAtUtc!.Value,
                    State = r.State.ToString(),
                    RemainingSeconds = (int)(r.ExpiresAtUtc.Value - now).TotalSeconds
                },
                pageModel,
                cancellationToken);
        }
    }
}