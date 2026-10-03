using Module.Catalog.Domain.Variants.Images;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogVariantImageSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 134;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<VariantImage>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoVariantImageJson>("007_demo_variant_images.json");
        if (json is null)
            return Result.Ok();

        foreach (var img in json)
        {
            var imageId = Guid.Parse(img.Id);
            var type = Enum.TryParse<VariantImageType>(img.Type, true, out var parsedType)
                ? parsedType
                : VariantImageType.Default;
            var imgResult = VariantImageMethod.Create(
                contentType: img.ContentType, fileName: img.FileName,
                fileSize: img.FileSize, url: $"/api/admin/catalog/variant-images/{imageId}/download",
                storagePath: img.StoragePath, position: img.Position, alt: img.Alt,
                type: type, variantId: Guid.Parse(img.VariantId));
            var image = imgResult.Value;
            image.Id = imageId;
            image.Width = img.Width;
            image.Height = img.Height;
            Context.Set<VariantImage>().Add(image);
        }
        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoVariantImageJson
    {
        public string Id { get; init; } = default!;
        public string VariantId { get; init; } = default!;
        public string ContentType { get; init; } = default!;
        public string FileName { get; init; } = default!;
        public int FileSize { get; init; }
        public int? Width { get; init; }
        public int? Height { get; init; }
        public string StoragePath { get; init; } = default!;
        public int Position { get; init; }
        public string Alt { get; init; } = default!;
        public string Type { get; init; } = default!;
    }
}
