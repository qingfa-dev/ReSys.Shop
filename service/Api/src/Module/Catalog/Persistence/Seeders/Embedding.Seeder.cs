using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogEmbeddingSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 135;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ImageEmbedding>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoEmbeddingJson>("demo_embeddings.json");
        if (json is null)
            return Result.Ok();

        foreach (var e in json)
        {
            var embedding = ImageEmbeddingMethod.Create(
                variantImageId: Guid.Parse(e.VariantImageId),
                modelName: e.ModelName,
                modelVersion: e.ModelVersion,
                vectorData: e.Vector);
            Context.Set<ImageEmbedding>().Add(embedding);
        }
        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private record DemoEmbeddingJson(string VariantImageId, string ModelName, string ModelVersion,
        float[] Vector, int Dimensions);
}
