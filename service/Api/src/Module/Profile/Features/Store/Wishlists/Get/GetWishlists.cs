using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.Get;

public static partial class GetWishlists
{
    public sealed record Query(Guid UserId, Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var page = request.Parameters.PageNumber ?? 1;
            var pageSize = request.Parameters.PageSize ?? 10;

            var query = dbContext.Set<Wishlist>()
                .Where(w => w.UserId == request.UserId && !w.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

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
