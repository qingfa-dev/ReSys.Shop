using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.Get;

/// <summary>Retrieves a paged list of wishlists for the authenticated user.</summary>
public static partial class GetWishlists
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Filters by user ownership, orders by creation date descending, and applies pagination.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of wishlist response items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=paged wishlists returned (may be empty)
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: 10, totalCount: 0);

            var page = request.Parameters.PageNumber ?? 1;
            var pageSize = request.Parameters.PageSize ?? 10;

            // Load: Non-deleted wishlists for current user
            var query = dbContext.Set<Wishlist>()
                .Where(w => w.UserId == Guid.Parse(currentUser.UserId) && !w.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(w => w.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new Response
                {
                    Id = w.Id,
                    Name = w.Name,
                    IsPrivate = w.IsPrivate,
                    IsDefault = w.IsDefault,
                    ItemCount = w.WishedItems.Count
                })
                .ToListAsync(cancellationToken);

            return PagedResult<Response>.Create(items: items, page: page, pageSize: pageSize, totalCount: totalCount);
        }
    }
}
