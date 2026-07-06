using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 120;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasTaxons = await HasDataAsync<Taxon>(cancellationToken);
        if (hasTaxons)
        {
            return Result.Ok();
        }

        var categoriesTaxonomy = await Context.Set<Taxonomy>()
            .FirstOrDefaultAsync(t => t.Name == "Categories", cancellationToken);

        var brandsTaxonomy = await Context.Set<Taxonomy>()
            .FirstOrDefaultAsync(t => t.Name == "Brands", cancellationToken);

        if (categoriesTaxonomy is null || brandsTaxonomy is null)
        {
            return Result.Ok();
        }

        var rootCategories = CreateRootTaxon(
            taxonomyId: categoriesTaxonomy.Id,
            parentId: null,
            name: "Categories",
            presentation: "All Categories",
            slug: "categories",
            lft: 1, rgt: 8, depth: 0);

        var men = CreateChildTaxon(
            taxonomyId: categoriesTaxonomy.Id,
            parentId: rootCategories.Id,
            name: "Men",
            presentation: "Men",
            slug: "men",
            lft: 2, rgt: 3, depth: 1);

        var women = CreateChildTaxon(
            taxonomyId: categoriesTaxonomy.Id,
            parentId: rootCategories.Id,
            name: "Women",
            presentation: "Women",
            slug: "women",
            lft: 4, rgt: 5, depth: 1);

        var accessories = CreateChildTaxon(
            taxonomyId: categoriesTaxonomy.Id,
            parentId: rootCategories.Id,
            name: "Accessories",
            presentation: "Accessories",
            slug: "accessories",
            lft: 6, rgt: 7, depth: 1);

        var rootBrands = CreateRootTaxon(
            taxonomyId: brandsTaxonomy.Id,
            parentId: null,
            name: "Brands",
            presentation: "All Brands",
            slug: "brands",
            lft: 1, rgt: 12, depth: 0);

        var nike = CreateChildTaxon(
            taxonomyId: brandsTaxonomy.Id,
            parentId: rootBrands.Id,
            name: "Nike",
            presentation: "Nike",
            slug: "nike",
            lft: 2, rgt: 3, depth: 1);

        var adidas = CreateChildTaxon(
            taxonomyId: brandsTaxonomy.Id,
            parentId: rootBrands.Id,
            name: "Adidas",
            presentation: "Adidas",
            slug: "adidas",
            lft: 4, rgt: 5, depth: 1);

        var zara = CreateChildTaxon(
            taxonomyId: brandsTaxonomy.Id,
            parentId: rootBrands.Id,
            name: "Zara",
            presentation: "Zara",
            slug: "zara",
            lft: 6, rgt: 7, depth: 1);

        var hm = CreateChildTaxon(
            taxonomyId: brandsTaxonomy.Id,
            parentId: rootBrands.Id,
            name: "H&M",
            presentation: "H&M",
            slug: "h-m",
            lft: 8, rgt: 9, depth: 1);

        var uniqlo = CreateChildTaxon(
            taxonomyId: brandsTaxonomy.Id,
            parentId: rootBrands.Id,
            name: "Uniqlo",
            presentation: "Uniqlo",
            slug: "uniqlo",
            lft: 10, rgt: 11, depth: 1);

        Context.Set<Taxon>().AddRange(
            rootCategories, men, women, accessories,
            rootBrands, nike, adidas, zara, hm, uniqlo);

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private static Taxon CreateRootTaxon(
        Guid taxonomyId,
        Guid? parentId,
        string name,
        string presentation,
        string slug,
        int lft,
        int rgt,
        int depth)
    {
        var result = TaxonMethod.Create(
            taxonomyId: taxonomyId,
            parentId: parentId,
            name: name,
            presentation: presentation,
            description: null,
            position: 0,
            slug: slug,
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: false,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: false,
            imageUrl: null,
            squareImageUrl: null);

        var taxon = result.Value;
        taxon.Lft = lft;
        taxon.Rgt = rgt;
        taxon.Depth = depth;
        taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
        taxon.CreatedBy = "System";

        return taxon;
    }

    private static Taxon CreateChildTaxon(
        Guid taxonomyId,
        Guid? parentId,
        string name,
        string presentation,
        string slug,
        int lft,
        int rgt,
        int depth)
    {
        var result = TaxonMethod.Create(
            taxonomyId: taxonomyId,
            parentId: parentId,
            name: name,
            presentation: presentation,
            description: null,
            position: 0,
            slug: slug,
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            automatic: false,
            rulesMatchPolicy: null,
            sortOrder: null,
            hideFromNav: false,
            imageUrl: null,
            squareImageUrl: null);

        var taxon = result.Value;
        taxon.Lft = lft;
        taxon.Rgt = rgt;
        taxon.Depth = depth;
        taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
        taxon.CreatedBy = "System";

        return taxon;
    }
}
