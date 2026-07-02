using Shared.Security.Authentication.External.Services;

namespace Module.Identity.Features.Store.Auth.External.Providers;

/// <summary>
/// Defines the use case for retrieving available external login providers.
/// </summary>
public static partial class ExternalProviders
{
    public sealed record PagedQuery : IPagedQuery<Response>;

    public class QueryHandler(ExternalProviderRegistry discoveryService)
        : IPagedQueryHandler<PagedQuery, Response>
    {
        // Contract: pre=request!=null, post=result!=null
        /// <summary>
        /// Handles the query to retrieve available external login providers.
        /// </summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the list of available external providers.</returns>
        public Task<PagedResult<Response>> Handle(PagedQuery request, CancellationToken cancellationToken)
        {
            // Query: Retrieve available external login providers
            var providers = discoveryService.GetAvailableProviders();

            // Map: Return the provider list as the response
            var result = PagedResult<Response>.Ok(
                items: providers.Items.Select(p => new Response
                {
                    Provider = p.Provider,
                    Options = p.Options
                }),
                page: 1,
                pageSize: providers.PageSize,
                totalCount: providers.TotalCount);

            return Task.FromResult(result);
        }
    }
}