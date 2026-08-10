using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Storefront.Products.Images.Get;

/// <summary>
/// Defines the use case for retrieving a product variant image.
/// </summary>
public static partial class GetImageById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IStorageService storageService)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Retrieves a product variant image by resolving the storage path and verifying the file exists on disk.
        /// </summary>
        /// <param name="query">The query containing the image ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the image full path and content type.</returns>
        // Contract: pre=query.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch image entity to retrieve storage path and content metadata
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            // Check: Return 404 if image does not exist
            if (image is null)
                return VariantImageResult.Failure.ById(query.Id);

            // Call: Download the binary file from the storage provider using stored path
            var downloadResult = await storageService.DownloadAsync(image.StoragePath, ct: cancellationToken);
            if (downloadResult.IsFailure)
                return downloadResult.Errors;
            // Map: Entity metadata combined with binary stream into download response

            return image.MapToStoreDownloadItem<Response>(downloadResult.Value.Content);
        }
    }
}