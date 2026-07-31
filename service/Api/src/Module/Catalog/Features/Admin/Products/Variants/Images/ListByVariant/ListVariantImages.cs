using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

/// <summary>
/// Defines the use case for listing images by variant.
/// </summary>
public static partial class ListVariantImages
{
    public sealed record Query(Guid VariantId, Parameters Parameters) : IPagedQuery<VariantImageDetailResponse>;

    /// <summary>
    /// Handles listing all images for a given variant, ordered by display position.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, VariantImageDetailResponse>
    {
        /// <summary>
        /// Executes the query: loads images for the variant ordered by position, paged or all in one page.
        /// </summary>
        // Contract: pre=query!=null, post=result!=null
        public async Task<PagedResult<VariantImageDetailResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            var pageModel = PageModelExtensions.FromValues(query.Parameters.PageNumber, query.Parameters.PageSize).Value;

            // Filter: Load images scoped to the variant, ordered by display position
            return await dbContext.Set<VariantImage>()
                .Where(x => x.VariantId == query.VariantId)
                .OrderBy(x => x.Position)
                .ToPagedOrAllAsync(x => x.MapToDetail<VariantImageDetailResponse>(), pageModel, cancellationToken);
        }
    }
}
