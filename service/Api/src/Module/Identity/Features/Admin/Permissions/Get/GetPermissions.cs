using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;

namespace Module.Identity.Features.Admin.Permissions.Get;

/// <summary>
/// Defines the use case for retrieving all available permissions.
/// </summary>
public static partial class GetPermissions
{
    public sealed record Query : IPagedQuery<PermissionMetadata>;

    public sealed class QueryHandler : IPagedQueryHandler<Query, PermissionMetadata>
    {
        /// <summary>
        /// Handles the query to retrieve all available permissions.
        /// </summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing all permission metadata.</returns>
        public Task<PagedResult<PermissionMetadata>> Handle(Query request, CancellationToken cancellationToken)
        {
            var all = PermissionContext.All;

            return Task.FromResult(PagedResult<PermissionMetadata>.Create(
                items: all,
                page: 1,
                pageSize: all.Count,
                totalCount: all.Count));
        }
    }
}