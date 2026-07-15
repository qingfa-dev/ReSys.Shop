using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogDemoSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasProducts = await HasDataAsync<Product>(cancellationToken);
        if (hasProducts)
            return Result.Ok();

        var jsonProducts = jsonHelper.LoadIfExists<DemoProductJson>("demo_products.json");
        var jsonVariants = jsonHelper.LoadIfExists<DemoVariantJson>("demo_variants.json");
        var jsonImages = jsonHelper.LoadIfExists<DemoVariantImageJson>("demo_variant_images.json");
        var jsonAssignments = jsonHelper.LoadIfExists<DemoOptionAssignmentJson>("demo_option_assignments.json");

        if (jsonProducts is null || jsonVariants is null)
            return Result.Ok();

        await SeedFromJsonAsync(jsonProducts, jsonVariants, jsonImages, jsonAssignments, cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(
        DemoProductJson[] products, DemoVariantJson[] variants,
        DemoVariantImageJson[]? images, DemoOptionAssignmentJson[]? assignments, CancellationToken ct)
    {
        var optionValues = await Context.Set<OptionValue>().ToListAsync(ct);
        var optionTypes = await Context.Set<OptionType>().ToListAsync(ct);

        var colorTypeId = optionTypes.FirstOrDefault(o => o.Name == "Color")?.Id;
        var sizeTypeId = optionTypes.FirstOrDefault(o => o.Name == "Size")?.Id;

        var taxonLookup = await Context.Set<Taxon>()
            .Where(t => !t.IsDeleted).ToDictionaryAsync(t => t.Slug, ct);

        foreach (var pj in products)
        {
            var productResult = ProductMethod.Create(
                name: pj.Name, slug: pj.Slug, description: pj.Description,
                status: ProductStatus.Active, availableOn: DateTimeOffset.UtcNow,
                metaTitle: pj.MetaTitle, metaDescription: pj.Description,
                metaKeywords: pj.MetaKeywords, id: Guid.Parse(pj.Id));
            var product = productResult.Value;
            product.GenderTarget = pj.GenderTarget;

            product.MasterVariantId = Guid.Parse(pj.MasterVariantId);

            Context.Set<Product>().Add(product);

            if (colorTypeId is not null && sizeTypeId is not null)
            {
                var potColor = ProductOptionTypeMethod.Create(product.Id, colorTypeId.Value, 0);
                Context.Set<ProductOptionType>().Add(potColor.Value);
                var potSize = ProductOptionTypeMethod.Create(product.Id, sizeTypeId.Value, 1);
                Context.Set<ProductOptionType>().Add(potSize.Value);
            }
        }
        await Context.SaveChangesAsync(ct);

        foreach (var vj in variants)
        {
            var variantResult = VariantMethod.Create(
                productId: Guid.Parse(vj.ProductId), sku: vj.Sku,
                isMaster: vj.IsMaster, position: vj.Position,
                barcode: vj.Barcode, id: Guid.Parse(vj.Id));
            var variant = variantResult.Value;
            variant.Price = vj.Price;

            var priceResult = PriceMethod.Create(amount: vj.Price, currency: "USD", variantId: variant.Id);
            var price = priceResult.Value!;
            price.IsDefault = true;

            Context.Set<Variant>().Add(variant);
            Context.Set<Price>().Add(price);
        }
        await Context.SaveChangesAsync(ct);

        if (images is not null)
        {
            foreach (var img in images)
            {
                var type = img.Type == "Search" ? VariantImageType.Search : VariantImageType.Default;
                var imgResult = VariantImageMethod.Create(
                    contentType: img.ContentType, fileName: img.FileName,
                    fileSize: 1, url: string.Empty, storagePath: img.StoragePath,
                    position: img.Position, alt: img.Alt, type: type,
                    variantId: Guid.Parse(img.VariantId));
                var image = imgResult.Value;
                image.Id = Guid.Parse(img.Id);
                Context.Set<VariantImage>().Add(image);
            }
            await Context.SaveChangesAsync(ct);
        }

        if (assignments is not null)
        {
            foreach (var a in assignments)
            {
                var ov = optionValues.FirstOrDefault(v =>
                    v.Name.Equals(a.OptionValueName, StringComparison.OrdinalIgnoreCase) &&
                    v.OptionTypeId == Guid.Parse(a.OptionTypeId));
                if (ov is null) continue;

                var assocResult = OptionValueVariantMethod.Create(
                    Guid.Parse(a.VariantId), ov.Id);
                if (assocResult.IsSuccess)
                    Context.Set<OptionValueVariant>().Add(assocResult.Value);
            }
            await Context.SaveChangesAsync(ct);
        }
    }

    private record DemoProductJson(string Id, string Name, string Slug, string Description, string Status,
        string GenderTarget, string MetaTitle, string MetaKeywords, string MasterVariantId);
    private record DemoVariantJson(string Id, string ProductId, string Sku, bool IsMaster, int Position,
        decimal Price, string? Barcode);
    private record DemoVariantImageJson(string Id, string VariantId, string ContentType, string FileName,
        string StoragePath, int Position, string Alt, string Type);
    private record DemoOptionAssignmentJson(string VariantId, string OptionValueName, string OptionTypeId);
}
