using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

namespace Module.Location.Features.Storefront.Countries.GetByIsoCode;

/// <summary>Retrieves a country by ISO code for the storefront.</summary>
public static partial class GetStorefrontCountryByIso
{
    public sealed record Query(string IsoCode) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single country by ISO code for storefront display.</summary>
        /// <param name="request">The query containing the ISO code.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the country details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=country found or NotFound returned
            // Load: Retrieve country by ISO code.
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