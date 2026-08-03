using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Module.Location.Features.Storefront.Countries.GetPagedOrAll;

/// <summary>Retrieves paged countries for the storefront with filtering and sorting.</summary>
public static partial class GetStorefrontCountryPagedOrAll
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads and paginates countries for storefront display.</summary>
        /// <param name="request">The query containing paging parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of country list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=paged result returned
            var parameters = request.Parameters;

            var parsing = parameters.ParseAll(
                allowedFilterFields: CountryConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: CountryConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: CountryConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return PagedResult<Response>.Create(errors: parsing.Errors);

            // Load: Retrieve countries with querying options.
            var pagedResult = await dbContext.Set<Country>()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(m => m.MapToListItem<Response>(), parsing.Value.Page, cancellationToken);

            // Map: Return paged result.
            return pagedResult;
        }
    }
}