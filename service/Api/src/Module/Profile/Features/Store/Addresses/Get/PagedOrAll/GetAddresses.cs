using Module.Profile.Domain;
using Module.Profile.Features.Store.Addresses.Shared.Mappings;

namespace Module.Profile.Features.Store.Addresses.Get.PagedOrAll;

/// <summary>Retrieves paged addresses for the authenticated user with optional type filtering.</summary>
public static partial class GetAddresses
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads the user profile and returns addresses filtered by type with in-memory pagination.</summary>
        /// <param name="request">The query containing paging and optional address-type filter.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of address response items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=paged addresses returned (may be empty)
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: 10, totalCount: 0);

            // Load: Get the profile for the current user
            var profile = await dbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(currentUser.UserId), cancellationToken);

            if (profile is null)
                return PagedResult<Response>.Create(items: [], page: 1, pageSize: 10, totalCount: 0);

            // Filter: Apply optional address type filter in memory
            var query = profile.Addresses.AsQueryable();

            if (request.Parameters.AddressType.HasValue)
            {
                query = query.Where(a => a.AddressType == request.Parameters.AddressType.Value);
            }

            // Pagination: Apply paging logic in memory
            var totalCount = query.Count();
            var page = request.Parameters.PageNumber ?? 1;
            var pageSize = request.Parameters.PageSize ?? 10;

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => a.ToResponse<Response>())
                .ToList();

            return PagedResult<Response>.Create(items: items, page: page, pageSize: pageSize, totalCount: totalCount);
        }
    }
}
