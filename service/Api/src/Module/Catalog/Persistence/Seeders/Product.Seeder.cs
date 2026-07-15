using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogDemoSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasProducts = await HasDataAsync<Product>(cancellationToken);
        if (hasProducts)
            return Result.Ok();

        var jsonProducts = DemoJsonHelper.LoadIfExists<DemoProductJson>("demo_products.json");
        var jsonVariants = DemoJsonHelper.LoadIfExists<DemoVariantJson>("demo_variants.json");
        var jsonImages = DemoJsonHelper.LoadIfExists<DemoVariantImageJson>("demo_variant_images.json");
        var jsonAssignments = DemoJsonHelper.LoadIfExists<DemoOptionAssignmentJson>("demo_option_assignments.json");

        if (jsonProducts is not null && jsonVariants is not null)
        {
            await SeedFromJsonAsync(jsonProducts, jsonVariants, jsonImages, jsonAssignments, cancellationToken);
            return Result.Ok();
        }

        await SeedHardcodedAsync(cancellationToken);
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

    private async Task SeedHardcodedAsync(CancellationToken ct)
    {
        // Existing hardcoded seeder logic preserved verbatim
        var menTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "men", ct);
        var womenTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "women", ct);
        var accessoriesTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "accessories", ct);
        if (menTaxon is null && womenTaxon is null && accessoriesTaxon is null) return;

        await SeedProductWithVariants(("Classic Cotton T-Shirt", "classic-cotton-t-shirt", "A comfortable classic cotton t-shirt.", "Classic Cotton T-Shirt", "t-shirt, cotton", menTaxon, "TEE-CTN-001-MSTR", "TEE-CTN-001-MSTR-BAR", 29.99m, null, [("S", "TEE-CTN-001-S"), ("M", "TEE-CTN-001-M"), ("L", "TEE-CTN-001-L"), ("XL", "TEE-CTN-001-XL")]), ct);
        await SeedProductWithVariants(("Slim Fit Jeans", "slim-fit-jeans", "Modern slim-fit jeans.", "Slim Fit Jeans", "jeans, denim", menTaxon, "JNS-SLM-001-MSTR", "JNS-SLM-001-MSTR-BAR", 79.99m, null, [("30", "JNS-SLM-001-30"), ("32", "JNS-SLM-001-32"), ("34", "JNS-SLM-001-34")]), ct);
        await SeedProductWithVariants(("Floral Summer Dress", "floral-summer-dress", "Light and breezy floral dress.", "Floral Summer Dress", "dress, floral", womenTaxon, "DRS-FLR-001-MSTR", "DRS-FLR-001-MSTR-BAR", 59.99m, 49.99m, [("S", "DRS-FLR-001-S"), ("M", "DRS-FLR-001-M"), ("L", "DRS-FLR-001-L")]), ct);
        await SeedProductWithoutSizes(("Leather Tote Bag", "leather-tote-bag", "Handcrafted genuine leather tote bag.", "Leather Tote Bag", "bag, tote", accessoriesTaxon, "BAG-LEA-001", "BAG-LEA-001-BAR", 129.99m), ct);
        await SeedProductWithVariants(("Running Sneakers", "running-sneakers", "Lightweight performance running shoes.", "Running Sneakers", "sneakers, running", menTaxon, "SNK-RUN-001-MSTR", "SNK-RUN-001-MSTR-BAR", 89.99m, 74.99m, [("8", "SNK-RUN-001-8"), ("9", "SNK-RUN-001-9"), ("10", "SNK-RUN-001-10")]), ct);
        await Context.SaveChangesAsync(ct);
    }

    private async Task SeedProductWithVariants((string Name, string Slug, string Description, string MetaTitle, string MetaKeywords, Taxon? Taxon, string MasterSku, string MasterBarcode, decimal Price, decimal? CompareAtPrice, (string Size, string Sku)[]? Sizes) seed, CancellationToken ct)
    {
        if (seed.Taxon is null) return;
        var productId = Guid.NewGuid(); var variantId = Guid.NewGuid();
        var productResult = ProductMethod.Create(seed.Name, seed.Slug, seed.Description, ProductStatus.Active, DateTimeOffset.UtcNow, seed.MetaTitle, seed.Description, seed.MetaKeywords, id: productId);
        var product = productResult.Value; product.GenderTarget = seed.Taxon.Name;
        var masterResult = VariantMethod.Create(productId, seed.MasterSku, true, 0, seed.MasterBarcode, id: variantId);
        var masterVariant = masterResult.Value; masterVariant.Price = seed.Price;
        var masterPriceResult = PriceMethod.Create(seed.Price, "USD", variantId, seed.CompareAtPrice, "US");
        masterPriceResult.Value!.IsDefault = true;
        var classificationResult = ClassificationMethod.Create(productId, seed.Taxon.Id, 0);
        product.Variants.Add(masterVariant); product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);
        Context.Set<Product>().Add(product); Context.Set<Variant>().Add(masterVariant); Context.Set<Price>().Add(masterPriceResult.Value);
        int pos = 1;
        foreach (var (size, sku) in seed.Sizes!)
        {
            var childVariantId = Guid.NewGuid();
            var childResult = VariantMethod.Create(productId, sku, false, pos, $"{sku}-BAR", id: childVariantId);
            var childVariant = childResult.Value; childVariant.Price = seed.Price;
            var childPriceResult = PriceMethod.Create(seed.Price, "USD", childVariantId, seed.CompareAtPrice, "US");
            product.Variants.Add(childVariant); Context.Set<Variant>().Add(childVariant); Context.Set<Price>().Add(childPriceResult.Value); pos++;
        }
    }

    private async Task SeedProductWithoutSizes((string Name, string Slug, string Description, string MetaTitle, string MetaKeywords, Taxon? Taxon, string MasterSku, string MasterBarcode, decimal Price) seed, CancellationToken ct)
    {
        if (seed.Taxon is null) return;
        var productId = Guid.NewGuid(); var variantId = Guid.NewGuid();
        var productResult = ProductMethod.Create(seed.Name, seed.Slug, seed.Description, ProductStatus.Active, DateTimeOffset.UtcNow, seed.MetaTitle, seed.Description, seed.MetaKeywords, id: productId);
        var product = productResult.Value; product.GenderTarget = "Unisex";
        var variantResult = VariantMethod.Create(productId, seed.MasterSku, true, 0, seed.MasterBarcode, id: variantId);
        var variant = variantResult.Value; variant.Price = seed.Price;
        var priceResult = PriceMethod.Create(seed.Price, "USD", variantId, compareAtAmount: null, "US");
        priceResult.Value!.IsDefault = true;
        var classificationResult = ClassificationMethod.Create(productId, seed.Taxon.Id, 0);
        product.Variants.Add(variant); product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);
        Context.Set<Product>().Add(product); Context.Set<Variant>().Add(variant); Context.Set<Price>().Add(priceResult.Value);
    }

    private record DemoProductJson(string Id, string Name, string Slug, string Description, string Status,
        string GenderTarget, string MetaTitle, string MetaKeywords, string MasterVariantId);
    private record DemoVariantJson(string Id, string ProductId, string Sku, bool IsMaster, int Position,
        decimal Price, string? Barcode);
    private record DemoVariantImageJson(string Id, string VariantId, string ContentType, string FileName,
        string StoragePath, int Position, string Alt, string Type);
    private record DemoOptionAssignmentJson(string VariantId, string OptionValueName, string OptionTypeId);
}
