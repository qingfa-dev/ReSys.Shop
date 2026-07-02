using Shared.Operational.Persistence.Specifications.Querying;
using Shared.Operational.Persistence.Data;

using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Location.Features.Store.Countries.GetPagedOrAll;

/// <summary>Handles paged or all retrieval of countries for storefront.</summary>
public static partial class GetStorefrontCountryPagedOrAll
{
    /// <summary>Query to retrieve a paged or all countries for the storefront.</summary>
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Executes the paged countries query for storefront.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of countries.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Query: Retrieve countries with querying options.
            var pagedResult = await dbContext.Set<Country>()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}