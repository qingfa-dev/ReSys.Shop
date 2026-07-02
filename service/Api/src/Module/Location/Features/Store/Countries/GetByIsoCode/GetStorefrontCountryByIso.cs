using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

namespace Module.Location.Features.Store.Countries.GetByIsoCode;

/// <summary>Handles retrieval of a country by ISO code for storefront.</summary>
public static partial class GetStorefrontCountryByIso
{
    /// <summary>Query to retrieve a country by ISO code for the storefront.</summary>
    public sealed record Query(string IsoCode) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get country by iso query for storefront.</summary>
        /// <param name="request">The query containing the ISO code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the country details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve country by ISO code.
            var entity = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c =>
                    c.IsoCode == request.IsoCode, cancellationToken: cancellationToken);

            if (entity is null)
                return CountryResult.Failure.NotFound;

            // Map: Return the country as response.
            return entity.MapToDetail<Response>();
        }
    }
}