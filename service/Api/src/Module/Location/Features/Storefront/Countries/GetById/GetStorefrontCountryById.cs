using Module.Location.Domain.Countries;
using Module.Location.Features.Shared.Countries.Mappings;

namespace Module.Location.Features.Storefront.Countries.GetById;

/// <summary>Retrieves a country by identifier for the storefront.</summary>
public static partial class GetStorefrontCountryById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single country by ID for storefront display.</summary>
        /// <param name="request">The query containing the country identifier.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the country details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=country found or NotFound returned
            // Load: Retrieve country by identifier.
            var entity = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == request.Id, cancellationToken: cancellationToken);

            if (entity is null)
                return CountryResult.Failure.NotFound;

            // Map: Return the country as response.
            return entity.MapToDetail<Response>();
        }
    }
}