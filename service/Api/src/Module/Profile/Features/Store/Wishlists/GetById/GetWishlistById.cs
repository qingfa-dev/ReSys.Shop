using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.GetById;

public static partial class GetWishlistById
{
    public sealed record Query(Guid UserId, Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == request.Id && w.UserId == request.UserId && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            return wishlist.MapToDetail<Response>();
        }
    }
}
