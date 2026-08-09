using Module.Identity.Features.Shared.Admin.Users.Shared.Mappings;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.GetById;

public static partial class GetUserById
{
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Handles the <see cref="Query"/> to retrieve a user by their ID.
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        // Contract: pre=request!=null, post=result!=null, throws=DbUpdateException
        /// <summary>
        /// Retrieves a user's full details by ID, or returns NotFound if no matching user exists.
        /// </summary>
        /// <param name="request">The query containing the user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the user details or NotFound error.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database query fails.</exception>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Load: Query the user by ID from the database
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            // Check: Return NotFound if no user matches the requested ID
            if (user is null)
                return UserResult.Failure.NotFound;

            // Transform: Map domain entity to response for API consumption
            var response = user.MapToDetail<Response>();

            return response;
        }
    }
}