using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

using Shared.Operational.Persistence.Data;

using Microsoft.EntityFrameworkCore;

namespace Module.Location.Features.Store.States.GetByIsoCode;

/// <summary>Handles retrieval of a state by ISO code for storefront.</summary>
public static partial class GetStorefrontStateByIso
{
    /// <summary>Query to retrieve a state by ISO code for the storefront.</summary>
    public sealed record Query(string IsoCode) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get state by iso query for storefront.</summary>
        /// <param name="request">The query containing the ISO code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the state details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve state by abbreviation.
            var entity = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s =>
                    s.Abbreviation == request.IsoCode, cancellationToken: cancellationToken);

            if (entity is null)
                return StateResult.Errors.NotFound;

            // Map: Return the state as response.
            return entity.MapToDetail<Response>();
        }
    }
}