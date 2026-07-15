using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 120;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasTaxons = await HasDataAsync<Taxon>(cancellationToken);
        if (hasTaxons)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoTaxonJson>("demo_taxons.json");
        if (json is not null)
        {
            await SeedFromJsonAsync(json, cancellationToken);
            return Result.Ok();
        }

        await SeedHardcodedAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(DemoTaxonJson[] items, CancellationToken ct)
    {
        foreach (var item in items)
        {
            Guid? parentId = string.IsNullOrEmpty(item.ParentId) ? null : Guid.Parse(item.ParentId);
            var result = TaxonMethod.Create(
                taxonomyId: Guid.Parse(item.TaxonomyId), parentId: parentId,
                name: item.Name, presentation: item.Presentation ?? item.Name,
                description: null, position: item.Position,
                slug: item.Slug, metaTitle: null, metaDescription: null, metaKeywords: null,
                automatic: false, rulesMatchPolicy: null, sortOrder: null, hideFromNav: false,
                imageUrl: null, squareImageUrl: null);

            var taxon = result.Value;
            taxon.Id = Guid.Parse(item.Id);
            taxon.Lft = item.Lft;
            taxon.Rgt = item.Rgt;
            taxon.Depth = item.Depth;
            taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
            taxon.CreatedBy = "System";

            Context.Set<Taxon>().Add(taxon);
        }

        await Context.SaveChangesAsync(ct);
    }

    private async Task SeedHardcodedAsync(CancellationToken ct)
    {
        var categoriesTaxonomy = await Context.Set<Taxonomy>().FirstOrDefaultAsync(t => t.Name == "Categories", ct);
        var brandsTaxonomy = await Context.Set<Taxonomy>().FirstOrDefaultAsync(t => t.Name == "Brands", ct);
        if (categoriesTaxonomy is null || brandsTaxonomy is null) return;

        var rootCategories = CreateTaxon(categoriesTaxonomy.Id, null, "Categories", "All Categories", "categories", 1, 8, 0);
        var men = CreateTaxon(categoriesTaxonomy.Id, rootCategories.Id, "Men", "Men", "men", 2, 3, 1);
        var women = CreateTaxon(categoriesTaxonomy.Id, rootCategories.Id, "Women", "Women", "women", 4, 5, 1);
        var accessories = CreateTaxon(categoriesTaxonomy.Id, rootCategories.Id, "Accessories", "Accessories", "accessories", 6, 7, 1);

        var rootBrands = CreateTaxon(brandsTaxonomy.Id, null, "Brands", "All Brands", "brands", 1, 12, 0);
        var nike = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Nike", "Nike", "nike", 2, 3, 1);
        var adidas = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Adidas", "Adidas", "adidas", 4, 5, 1);
        var zara = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Zara", "Zara", "zara", 6, 7, 1);
        var hm = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "H&M", "H&M", "h-m", 8, 9, 1);
        var uniqlo = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Uniqlo", "Uniqlo", "uniqlo", 10, 11, 1);

        Context.Set<Taxon>().AddRange(rootCategories, men, women, accessories, rootBrands, nike, adidas, zara, hm, uniqlo);
        await Context.SaveChangesAsync(ct);
    }

    private static Taxon CreateTaxon(Guid taxonomyId, Guid? parentId, string name, string presentation, string slug, int lft, int rgt, int depth)
    {
        var result = TaxonMethod.Create(taxonomyId, parentId, name, presentation, null, 0, slug, null, null, null, false, null, null, false, null, null);
        var taxon = result.Value;
        taxon.Lft = lft; taxon.Rgt = rgt; taxon.Depth = depth;
        taxon.CreatedAtUtc = DateTimeOffset.UtcNow; taxon.CreatedBy = "System";
        return taxon;
    }

    private record DemoTaxonJson(string Id, string TaxonomyId, string? ParentId, string Name, string? Presentation,
        string Slug, int Depth, int Lft, int Rgt, int Position);
}
