using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Shared.Mappings;

namespace Module.Customer.Features.Storefront.Wishlists.Get;

/// <summary>Retrieves all wishlists for the current user.</summary>
public static partial class GetWishlists
{
    public sealed record Query(Guid UserId, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handles the retrieval of all wishlists for the current user.</summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Retrieves all wishlists for the current user.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var page = request.Parameters.PageNumber ?? 1;
            var pageSize = request.Parameters.PageSize ?? 10;

            // Load: Query non-deleted wishlists scoped to the current user
            var query = dbContext.Set<Wishlist>()
                .Where(w => w.UserId == request.UserId && !w.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            // Transform: Apply paging, ordering, and map to response DTOs
            var items = await query
                .OrderByDescending(w => w.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => w.MapToListItem<Response>())
                .ToListAsync(cancellationToken);

            return PagedResult<Response>.Create(items: items, page: page, pageSize: pageSize, totalCount: totalCount);
        }
    }
}
