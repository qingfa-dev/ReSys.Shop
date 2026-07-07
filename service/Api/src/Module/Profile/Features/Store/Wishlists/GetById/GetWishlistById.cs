using Microsoft.EntityFrameworkCore;

using Module.Profile.Domain.Wishlists;
using Module.Profile.Domain.Wishlists.WishedItems;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.GetById;

public static partial class GetWishlistById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

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
