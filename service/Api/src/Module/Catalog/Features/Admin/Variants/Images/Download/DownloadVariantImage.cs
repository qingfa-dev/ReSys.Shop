using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Shared.Mappings;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Admin.Variants.Images.Download;

/// <summary>
/// Defines the use case for downloading a variant image.
/// </summary>
public static partial class DownloadVariantImage
{
    public sealed record Query(Guid ImageId) : IQuery<Response>;

    /// <summary>
    /// Handles downloading the binary content of a variant image from storage.
    /// </summary>
    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IStorageService storageService)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Executes the query: loads image metadata, fetches file stream from storage, maps to response.
        /// </summary>
        /// <param name="query">The query containing the image ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A response with the file stream and content metadata, or a failure result.</returns>
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Load: Fetch image entity to retrieve storage path and content metadata
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(x => x.Id == query.ImageId, cancellationToken);

            // Check: Return 404 if image does not exist
            if (image is null)
                return VariantImageResult.Failure.ById(query.ImageId);

            // Call: Download the binary file from the storage provider using stored path
            var downloadResult = await storageService.DownloadAsync(image.StoragePath, ct: cancellationToken);
            if (downloadResult.IsFailure)
                return downloadResult.Errors;

            // Map: Entity metadata combined with binary stream into download response
            return Result<Response>.Ok(image.MapToDownload<Response>(downloadResult.Value.Content));
        }
    }
}