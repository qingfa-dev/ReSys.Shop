using Microsoft.AspNetCore.Identity;

using DomainRoles = Module.Identity.Domain.Roles;

using Module.Identity.Features.Admin.Shared.Mappings;

using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Shared.Admin.Roles.Get.PagedOrAll;

/// <summary>
/// Defines the use case for retrieving roles with paged or all results.
/// </summary>
public static partial class GetRolesPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve roles with paging or all results.
    /// </summary>
    public sealed class QueryHandler(RoleManager<Role> roleManager)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves roles with optional paging, filtering, searching, and ordering applied.
        /// </summary>
        /// <param name="request">The query containing pagination and filtering parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing role list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            // Validate: Parse and validate filter, search, and sort parameters against allowed fields
            var parsing = parameters.ParseAll(
                allowedFilterFields: DomainRoles.RoleConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: DomainRoles.RoleConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: DomainRoles.RoleConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Load: Access role queryable from the role manager
            var roles = roleManager.Roles;

            // Transform: Apply dynamic querying and projection, then paginate the results
            var pagedResult = await roles
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(r => r.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            return pagedResult;
        }
    }
}