using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Location.Features.Store.States.GetPagedOrAll;

/// <summary>Handles paged or all retrieval of states for storefront.</summary>
public static partial class GetStorefrontStatePagedOrAll
{
    /// <summary>Query to retrieve a paged or all states for the storefront.</summary>
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Executes the paged states query for storefront.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result of states.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Query: Retrieve states with querying options.
            var pagedResult = await dbContext.Set<State>()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}