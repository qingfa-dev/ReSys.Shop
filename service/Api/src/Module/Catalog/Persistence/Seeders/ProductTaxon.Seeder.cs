using Module.Catalog.Domain.Products.Classifications;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogProductTaxonSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 136;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Classification>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoClassificationJson>("008_demo_product_taxons.json");
        if (json is null)
            return Result.Ok();

        foreach (var c in json)
        {
            var result = ClassificationMethod.Create(
                Guid.Parse(c.ProductId), Guid.Parse(c.TaxonId),
                c.Position, isAutomatic: true);
            if (result.IsSuccess)
                Context.Set<Classification>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoClassificationJson
    {
        public string ProductId { get; init; } = default!;
        public string TaxonId { get; init; } = default!;
        public int Position { get; init; }
    }
}
