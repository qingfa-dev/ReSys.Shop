using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

using Shared.Operational.Persistence.Data;

using Microsoft.EntityFrameworkCore;

namespace Module.Location.Features.Admin.Countries.GetById;

/// <summary>Handles retrieval of a country by identifier.</summary>
public static partial class GetCountryById
{
    /// <summary>Query to retrieve a country by ID.</summary>
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get country by id query.</summary>
        /// <param name="request">The query containing the country identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the country details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve country by identifier.
            var entity = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == request.Id, cancellationToken: cancellationToken);

            if (entity is null)
                return CountryResult.Errors.NotFound;

            // Map: Return the country as response.
            return entity.MapToDetail<Response>();
        }
    }
}