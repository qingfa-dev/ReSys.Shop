using Module.Catalog.Domain.Variants.Images.Embeddings;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogEmbeddingSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 137;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        return await SeedFromJsonAsync(cancellationToken);
    }

    private async Task<Result> SeedFromJsonAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ImageEmbedding>(cancellationToken);
        if (hasData)
            return Result.Ok();

        // Load per-model JSON files from 012_demo_embeddings/ folder
        var basePath = jsonHelper.GetBasePath();
        var embeddingsDir = Path.Combine(basePath, "012_demo_embeddings");
        if (!Directory.Exists(embeddingsDir))
            return Result.Ok();

        var jsonFiles = Directory.GetFiles(embeddingsDir, "*.json");
        if (jsonFiles.Length == 0)
            return Result.Ok();

        int inserted = 0;
        int skipped = 0;

        foreach (var filePath in jsonFiles)
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var entries = System.Text.Json.JsonSerializer.Deserialize<DemoEmbeddingJson[]>(
                json, DemoJsonHelper.JsonOptions);

            if (entries is null)
                continue;

            foreach (var e in entries)
            {
                var modelName = e.ModelName;
                var vector = e.Vector;

                // Validate: vector dimension must match expected dimension for the model
                if (ImageEmbeddingConstant.VectorDimensions.TryGetValue(modelName, out var expectedDim))
                {
                    if (vector.Length != expectedDim)
                    {
                        skipped++;
                        continue;
                    }
                }

                var embedding = ImageEmbeddingMethod.Create(
                    variantImageId: Guid.Parse(e.VariantImageId),
                    modelName: modelName,
                    modelVersion: e.ModelVersion,
                    vectorData: vector);
                Context.Set<ImageEmbedding>().Add(embedding);
                inserted++;
            }
        }

        if (inserted > 0)
            await SaveChangesWithIdempotencyAsync(cancellationToken);

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
