using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Variants.Images.GetById;

/// <summary>
/// Defines the use case for retrieving a variant image by ID.
/// </summary>
public static partial class GetVariantImageById
{
    public sealed record Query(Guid ImageId) : IQuery<Response>;

    /// <summary>
    /// Handles retrieving a single variant image by its identifier.
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Executes the query: loads image by ID and maps to detail response.
        /// </summary>
        /// <param name="query">The query containing the image ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>The image detail, or a not-found failure.</returns>
        // Contract: pre=query!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch image entity from database by primary key
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(x => x.Id == query.ImageId, cancellationToken);

            // Check: Return 404 if no image matches the requested ID
            if (image is null)
                return VariantImageResult.Failure.ById(query.ImageId);

            // Map: Domain entity to wire-format detail response
            return Result<Response>.Ok(image.MapToDetail<Response>());
        }
    }
}