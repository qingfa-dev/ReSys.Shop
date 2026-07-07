using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.PromotionCategories;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionCategories.Get.ById;
/// <summary>Gets a promotion category by its ID.</summary>
public static partial class GetPromotionCategoryById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext) : IQueryHandler<Query, Response>
    {
        /// <summary>Handles retrieving a promotion category by ID.</summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The promotion category response.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Contract: pre=query!=null, post=result!=null
            // Query: Get promotion category by ID.
            var category = await dbContext.Set<PromotionCategory>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

            // Check: Verify the category exists.
            if (category is null)
                return PromotionCategoryResult.Errors.NotFound(query.Id);

            // Map: Return category details.
            return category.MapToDetail<Response>();
        }
    }
}
