using Module.Catalog.Domain.Products.Variants.Images;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    // EXCEPTION: image serving response — no domain entity
    public sealed record Response
    {
        public string FullPath { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
    }

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
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(i => i.Id == query.Id, cancellationToken);

            if (image is null)
                return VariantImageResult.Failure.ById(query.Id);

            var pathResult = await storageService.ResolvePathAsync(image.StoragePath, ct: cancellationToken);

            if (pathResult.IsFailure)
                return pathResult.Errors;

            var fullPath = pathResult.Value;

            if (!File.Exists(fullPath))
                return VariantImageResult.Failure.ById(query.Id);

            return new Response { FullPath = fullPath, ContentType = image.ContentType };
        }
    }
}