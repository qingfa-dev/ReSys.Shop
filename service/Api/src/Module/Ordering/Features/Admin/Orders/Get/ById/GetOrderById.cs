using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Get.ById;

    /// <summary>Handles GetOrderById feature.</summary>
    public static partial class GetOrderById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Query: Retrieve order by identifier with line items.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(request.Id);

            // Map: Convert entity to response DTO.
            return entity.MapToDetail<Response>();
        }
    }
}
