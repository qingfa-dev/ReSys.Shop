using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

/// <summary>
/// Defines the use case for listing images by variant.
/// </summary>
public static partial class ListVariantImages
{
    public sealed record Query(Guid VariantId) : IQuery<Response>;

    /// <summary>
    /// Handles listing all images for a given variant, ordered by display position.
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Executes the query: loads all images for the variant, ordered by position ascending.
        /// </summary>
        /// <param name="query">The query containing the variant ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A response containing the ordered image list.</returns>
        // Contract: pre=query!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Filter: Load all images scoped to the variant, ordered by display position
            var images = await dbContext.Set<VariantImage>()
                .Where(x => x.VariantId == query.VariantId)
                .OrderBy(x => x.Position)
                .ToListAsync(cancellationToken);

            // Map: Domain entities to wire-format detail DTOs
            return Result<Response>.Ok(new Response
            {
                Images = images.Select(x => x.MapToDetail<VariantImageDetailResponse>()).ToList()
            });
        }
    }
}