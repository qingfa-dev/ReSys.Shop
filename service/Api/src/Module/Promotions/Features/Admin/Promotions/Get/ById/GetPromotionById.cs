using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.Promotions.Get.ById;
/// <summary>Gets a promotion by its ID.</summary>
public static partial class GetPromotionById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        /// <summary>Handles retrieving a promotion by ID.</summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The promotion response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Query: Get promotion by ID.
            var promotion = await dbContext.Set<Promotion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

            // Check: Verify the promotion exists.
            if (promotion is null)
                return PromotionResult.Errors.NotFound(query.Id);

            // Map: Return promotion details.
            return promotion.MapToDetail<Response>();
        }
    }
}
