using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Storefront.Orders.Get.ById;

    /// <summary>Handles GetCustomerOrder feature.</summary>
    public static partial class GetCustomerOrder
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {

        // Contract: pre=query!=null, post=result!=null
            // Check: Resolve current user identifier.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return (Result<Response>)OrderResult.Errors.NotFound(query.Id);

            // Query: Retrieve order by identifier scoped to current user.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return (Result<Response>)OrderResult.Errors.NotFound(query.Id);

            // Map: Convert entity to response DTO.
            return entity.MapToDetail<Response>();
        }
    }
}
