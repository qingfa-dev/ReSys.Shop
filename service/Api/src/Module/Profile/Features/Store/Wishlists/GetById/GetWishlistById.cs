using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.GetById;

/// <summary>Retrieves a wishlist with its items for the authenticated user.</summary>
public static partial class GetWishlistById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Validates ownership, loads the wishlist with items ordered by creation date descending.</summary>
        /// <param name="request">The query containing the wishlist ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the wishlist with items or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=wishlist found or NotFound returned
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            // Load: Find the wishlist with items, owned by current user
            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == request.Id && w.UserId == Guid.Parse(currentUser.UserId) && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            return new Response
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                IsPrivate = wishlist.IsPrivate,
                IsDefault = wishlist.IsDefault,
                Token = wishlist.Token,
                ItemCount = wishlist.WishedItems.Count,
                WishedItems = wishlist.WishedItems
                    .OrderByDescending(i => i.CreatedAtUtc)
                    .Select(i => new WishedItemResponse
                    {
                        Id = i.Id,
                        VariantId = i.VariantId,
                        Quantity = i.Quantity,
                        AddedAtUtc = i.CreatedAtUtc
                    })
                    .ToList()
            };
        }
    }
}
