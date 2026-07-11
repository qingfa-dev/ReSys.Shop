using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Admin.States.GetByIsoCode;

/// <summary>Retrieves a state by its abbreviation (ISO code).</summary>
public static partial class GetStateByIso
{
    public sealed record Query(string IsoCode) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single state by abbreviation and maps to detail response.</summary>
        /// <param name="request">The query containing the ISO code.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the state details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=state found or NotFound returned
            // Load: Retrieve state by abbreviation.
            var entity = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s =>
                    s.Abbreviation == request.IsoCode, cancellationToken: cancellationToken);

            if (entity is null)
                return StateResult.Failure.NotFound;

            // Map: Return the state as response.
            return entity.MapToDetail<Response>();
        }
    }
}
