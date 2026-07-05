using Module.Catalog.Domain.Taxonomies;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonomySeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 110;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasTaxonomies = await HasDataAsync<Taxonomy>(cancellationToken);
        if (hasTaxonomies)
        {
            return Result.Ok();
        }

        var categoriesResult = TaxonomyExtensions.Create(
            name: "Categories",
            presentation: "Departments",
            position: 0,
            id: Guid.NewGuid());

        var brandsResult = TaxonomyExtensions.Create(
            name: "Brands",
            presentation: "Brands",
            position: 1,
            id: Guid.NewGuid());

        Context.Set<Taxonomy>().AddRange(categoriesResult.Value, brandsResult.Value);
        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
