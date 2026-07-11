using Shared.Security.Authentication.External.Services;

namespace Module.Identity.Features.Store.Auth.Login.External.Providers;

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
        /// Returns the list of configured external OAuth/OpenID providers available for login.
        /// </summary>
        /// <param name="request">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result containing available external providers.</returns>
        public Task<PagedResult<Response>> Handle(PagedQuery request, CancellationToken cancellationToken)
        {
            var providers = discoveryService.GetAvailableProviders();

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