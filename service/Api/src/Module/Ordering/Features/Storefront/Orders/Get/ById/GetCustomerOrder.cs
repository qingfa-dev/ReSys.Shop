using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Storefront.Orders.Get.ById;

/// <summary>Retrieves a placed order by ID scoped to the current customer, including line items.</summary>
public static partial class GetCustomerOrder
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Finds the order by ID scoped to the current user and maps it to a customer-safe detail response.</summary>
        /// <param name="query">The query containing the order ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The order detail response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Check: Resolve current user identifier.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return (Result<Response>)OrderResult.Failure.NotFound(query.Id);

            // Check: Retrieve order by identifier scoped to current user.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Id && x.UserId == userId, cancellationToken);

            if (entity is null)
                return (Result<Response>)OrderResult.Failure.NotFound(query.Id);

            // Map: Convert entity to response DTO.
            return entity.MapToDetail<Response>();
        }
    }
}
