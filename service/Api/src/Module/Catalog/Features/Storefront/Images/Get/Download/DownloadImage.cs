using Module.Catalog.Domain.Products.Variants.Images;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Storefront.Images.Get.Download;

public static partial class DownloadImage
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed record Response(Stream Stream, string FileName, string ContentType) : IDisposable
    {
        public void Dispose() => Stream.Dispose();
    }

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

            var downloadResult = await storageService.DownloadAsync(image.StoragePath, ct: cancellationToken);

            if (downloadResult.IsFailure)
                return downloadResult.Errors;

            return new Response(
                downloadResult.Value.Content,
                image.FileName,
                image.ContentType);
        }
    }
}
