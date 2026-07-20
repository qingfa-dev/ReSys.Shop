using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Permissions.Get;

/// <summary>
/// Defines the use case for retrieving all available permissions.
/// </summary>
public static partial class GetPermissions
{
    public sealed record Query : IPagedQuery<PermissionMetadata>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve all available permissions.
    /// </summary>
    public sealed class QueryHandler : IPagedQueryHandler<Query, PermissionMetadata>
    {
        // Contract: pre=request!=null, post=result!=null
        /// <summary>
        /// Returns every known permission from the permission registry, unfiltered — used for display and assignment UIs.
        /// </summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing all permission metadata.</returns>
        public Task<PagedResult<PermissionMetadata>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Fetch all registered permissions from the global permission registry
            var all = PermissionContext.All;

            // Transform: Package permissions into a paged result for uniform API response
            return Task.FromResult(PagedResult<PermissionMetadata>.Create(
                items: all,
                page: 1,
                pageSize: all.Count,
                totalCount: all.Count));
        }
    }
}