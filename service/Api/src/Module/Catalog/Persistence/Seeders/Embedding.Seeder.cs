using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogEmbeddingSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 135;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        return await SeedFromJsonAsync(cancellationToken);
    }

    private async Task<Result> SeedFromJsonAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ImageEmbedding>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoEmbeddingJson>("demo_embeddings.json");
        if (json is null)
            return Result.Ok();

        var imageIds = json.Select(e => Guid.Parse(e.VariantImageId)).Distinct().ToArray();
        var existingImageIds = await Context.Set<VariantImage>()
            .Where(vi => imageIds.Contains(vi.Id))
            .Select(vi => vi.Id)
            .ToHashSetAsync(cancellationToken);

        var existingEmbeddings = new HashSet<(Guid VariantImageId, string ModelName)>(
            (await Context.Set<ImageEmbedding>().ToListAsync(cancellationToken))
            .Select(em => (em.VariantImageId, em.ModelName)));

        foreach (var e in json)
        {
            var imageId = Guid.Parse(e.VariantImageId);
            if (!existingImageIds.Contains(imageId))
                continue;

            if (existingEmbeddings.Contains((imageId, e.ModelName)))
                continue;

            var embedding = ImageEmbeddingMethod.Create(
                variantImageId: imageId,
                modelName: e.ModelName,
                modelVersion: e.ModelVersion,
                vectorData: e.Vector);
            Context.Set<ImageEmbedding>().Add(embedding);
            existingEmbeddings.Add((imageId, e.ModelName));
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoEmbeddingJson
    {
        public string VariantImageId { get; init; } = default!;
        public string ModelName { get; init; } = default!;
        public string ModelVersion { get; init; } = default!;
        public float[] Vector { get; init; } = default!;
        public int Dimensions { get; init; }
    }
}
