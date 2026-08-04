using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

/// <summary>Lists active, non-expired stock reservations for a given cart token with remaining TTL.</summary>
public static partial class GetCartReservations
{
    public sealed record Query(string CartToken, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Fetches all active reservations for the cart and computes remaining seconds before expiry.</summary>
        /// <param name="request">The query containing the cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active reservations with remaining TTL.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var now = DateTimeOffset.UtcNow;
            var reservations = await dbContext.Set<StockReservation>()
                .Where(r => r.CartToken == request.CartToken
                            && r.State == ReservationState.Reserved
                            && r.ExpiresAtUtc > now)
                .ToListAsync(cancellationToken);

            // Map: Compute remaining seconds for each reservation
            var items = reservations.Select(r => new Response
            {
                Id = r.Id,
                VariantId = r.VariantId,
                Quantity = r.Quantity,
                ExpiresAtUtc = r.ExpiresAtUtc!.Value,
                State = r.State.ToString(),
                RemainingSeconds = (int)(r.ExpiresAtUtc!.Value - now).TotalSeconds
            }).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            var ordered = items.OrderBy(i => i.ExpiresAtUtc).ToList();

            // Transform: Return all in one page or honor caller-supplied paging
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(ordered, 1, Math.Max(1, ordered.Count), ordered.Count)
                : ordered.ToPagedResult(pageModel);
        }
    }
}