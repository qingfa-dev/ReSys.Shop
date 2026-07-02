using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Admin.States.GetById;

/// <summary>Handles retrieval of a state by identifier.</summary>
public static partial class GetStateById
{
    /// <summary>Query to retrieve a state by ID.</summary>
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Executes the get state by id query.</summary>
        /// <param name="request">The query containing the state identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the state details.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=result!=null
            // Query: Retrieve state by identifier.
            var state = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Id == request.Id, cancellationToken: cancellationToken);

            if (state is null)
                return StateResult.Failure.NotFound;

            // Map: Return the state as response.
            return state.MapToDetail<Response>();
        }
    }
}