using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Roles.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;
using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Roles.Get.PagedOrAll;

/// <summary>
/// Defines the use case for retrieving roles with paged or all results.
/// </summary>
public static partial class GetRolesPagedOrAll
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class QueryHandler(RoleManager<Role> roleManager)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null
        /// <summary>
        /// Handles the query to retrieve roles with paged or all results.
        /// </summary>
        /// <param name="request">The query containing pagination and filtering parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing role list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Query: Retrieve all roles from the role manager.
            var roles = roleManager.Roles;

            // Map: Apply querying options (pagination, filtering, searching, ordering) and map to response DTOs.
            var pagedResult = await roles
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(r => r.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            return pagedResult;
        }
    }
}
