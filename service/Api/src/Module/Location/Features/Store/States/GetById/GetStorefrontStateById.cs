using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Store.States.GetById;

/// <summary>Retrieves a state by identifier for the storefront.</summary>
public static partial class GetStorefrontStateById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single state by ID for storefront display.</summary>
        /// <param name="request">The query containing the state identifier.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the state details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=state found or NotFound returned
            // Load: Retrieve state by identifier.
            var state = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Id == request.Id, cancellationToken: cancellationToken);

            if (state is null)
                return StateResult.Failure.NotFound;

            // Map: Return the state as response.
            return state.MapToDetail<Response>();
        }
    }
}