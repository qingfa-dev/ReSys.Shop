using Module.Catalog.Domain.Products.Variants.Images;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed record Response(string FullPath, string ContentType);

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IStorageService storageService)
        : IQueryHandler<Query, Response>
    {
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

            return new Response(fullPath, image.ContentType);
        }
    }
}
