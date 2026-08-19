using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Shared.Mappings;

namespace Module.Customer.Features.Storefront.Wishlists.GetById;

/// <summary>Retrieves a wishlist by its identifier.</summary>
public static partial class GetWishlistById
{
    public sealed record Query(Guid UserId, Guid Id) : IQuery<Response>;

    /// <summary>Handles the retrieval of a wishlist by its identifier.</summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Retrieves a wishlist by its identifier.</summary>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch the wishlist with items from persistence
            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == request.Id && w.UserId == request.UserId && !w.IsDeleted,
                    cancellationToken);

            // Validate: Confirm wishlist exists
            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            // Transform: Map wishlist to detail response DTO
            return wishlist.MapToDetail<Response>();
        }
    }
}
