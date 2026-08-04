using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogProductSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Product>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoProductJson>("005_demo_products.json");
        if (json is null)
            return Result.Ok();

        var optionTypes = await Context.Set<OptionType>().ToListAsync(cancellationToken);
        var colorTypeId = optionTypes.FirstOrDefault(o => o.Name == "Color")?.Id;
        var sizeTypeId = optionTypes.FirstOrDefault(o => o.Name == "Size")?.Id;

        foreach (var pj in json)
        {
            var productResult = ProductMethod.Create(
            #region Properties
                name: pj.Name,
                description: pj.Description,
                status: Enum.TryParse<ProductStatus>(pj.Status, out var parsedStatus) ? parsedStatus : ProductStatus.Active,
            #endregion Properties
            #region SEO
                slug: pj.Slug,
                metaTitle: pj.MetaTitle,
                metaDescription: pj.MetaDescription,
                metaKeywords: pj.MetaKeywords,
            #endregion SEO
            #region Timestamp
                availableOn: DateTimeOffset.UtcNow.AddDays(-1),
                discontinueOn: DateTimeOffset.UtcNow.AddYears(1),
                makeActiveAt: DateTimeOffset.UtcNow.AddDays(-1),
            #endregion Timestamp
            #region Fashion
                styleCode: pj.StyleCode,
                seasonName: pj.SeasonName,
                materialComposition: pj.MaterialComposition,
                careInstructions: pj.CareInstructions,
                fitNotes: pj.FitNotes,
                department: pj.Department,
                genderTarget: pj.GenderTarget,
            #endregion Fashion
                id: Guid.Parse(pj.Id));
            var product = productResult.Value;
            product.GenderTarget = pj.GenderTarget;
            product.MasterVariantId = Guid.Parse(pj.MasterVariantId);

            Context.Set<Product>().Add(product);

            if (colorTypeId is not null && sizeTypeId is not null)
            {
                Context.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, colorTypeId.Value, 0).Value);
                Context.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, sizeTypeId.Value, 1).Value);
            }
        }
        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoProductJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string GenderTarget { get; init; } = default!;
        public string MetaTitle { get; init; } = default!;
        public string? MetaDescription { get; init; }
        public string MetaKeywords { get; init; } = default!;
        public string MasterVariantId { get; init; } = default!;
        public string? StyleCode { get; init; }
        public string? SeasonName { get; init; }
        public string? MaterialComposition { get; init; }
        public string? CareInstructions { get; init; }
        public string? FitNotes { get; init; }
        public string? Department { get; init; }
    }
}
