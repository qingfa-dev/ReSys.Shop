using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogDemoSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasProducts = await HasDataAsync<Product>(cancellationToken);
        if (hasProducts)
            return Result.Ok();

        var menTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "men", cancellationToken);
        var womenTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "women", cancellationToken);
        var accessoriesTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "accessories", cancellationToken);

        if (menTaxon is null && womenTaxon is null && accessoriesTaxon is null)
            return Result.Ok();

        await SeedProductWithVariants(new ProductSeed(
            Name: "Classic Cotton T-Shirt",
            Slug: "classic-cotton-t-shirt",
            Description: "A comfortable classic cotton t-shirt perfect for everyday wear. Made from 100% organic cotton with a relaxed fit.",
            MetaTitle: "Classic Cotton T-Shirt",
            MetaKeywords: "t-shirt, cotton, classic, casual",
            Taxon: menTaxon,
            MasterSku: "TEE-CTN-001-MSTR",
            MasterBarcode: "TEE-CTN-001-MSTR-BAR",
            Price: 29.99m,
            Sizes: [("S", "TEE-CTN-001-S"), ("M", "TEE-CTN-001-M"), ("L", "TEE-CTN-001-L"), ("XL", "TEE-CTN-001-XL")]
        ), cancellationToken);

        await SeedProductWithVariants(new ProductSeed(
            Name: "Slim Fit Jeans",
            Slug: "slim-fit-jeans",
            Description: "Modern slim-fit jeans crafted from stretch denim for all-day comfort. Features a classic five-pocket design.",
            MetaTitle: "Slim Fit Jeans",
            MetaKeywords: "jeans, denim, slim-fit, pants",
            Taxon: menTaxon,
            MasterSku: "JNS-SLM-001-MSTR",
            MasterBarcode: "JNS-SLM-001-MSTR-BAR",
            Price: 79.99m,
            Sizes: [("30", "JNS-SLM-001-30"), ("32", "JNS-SLM-001-32"), ("34", "JNS-SLM-001-34")]
        ), cancellationToken);

        await SeedProductWithVariants(new ProductSeed(
            Name: "Floral Summer Dress",
            Slug: "floral-summer-dress",
            Description: "A light and breezy floral print dress perfect for warm days. Features adjustable straps and a flowing A-line silhouette.",
            MetaTitle: "Floral Summer Dress",
            MetaKeywords: "dress, floral, summer, women",
            Taxon: womenTaxon,
            MasterSku: "DRS-FLR-001-MSTR",
            MasterBarcode: "DRS-FLR-001-MSTR-BAR",
            Price: 59.99m,
            CompareAtPrice: 49.99m,
            Sizes: [("S", "DRS-FLR-001-S"), ("M", "DRS-FLR-001-M"), ("L", "DRS-FLR-001-L")]
        ), cancellationToken);

        await SeedProductWithoutSizes(new ProductSeed(
            Name: "Leather Tote Bag",
            Slug: "leather-tote-bag",
            Description: "Handcrafted genuine leather tote bag with gold-tone hardware. Features a spacious main compartment and interior zip pocket.",
            MetaTitle: "Leather Tote Bag",
            MetaKeywords: "bag, tote, leather, accessories",
            Taxon: accessoriesTaxon,
            MasterSku: "BAG-LEA-001",
            MasterBarcode: "BAG-LEA-001-BAR",
            Price: 129.99m
        ), cancellationToken);

        await SeedProductWithVariants(new ProductSeed(
            Name: "Running Sneakers",
            Slug: "running-sneakers",
            Description: "Lightweight performance running shoes with responsive cushioning and breathable mesh upper. Designed for road running.",
            MetaTitle: "Running Sneakers",
            MetaKeywords: "sneakers, running, shoes, athletic",
            Taxon: menTaxon,
            MasterSku: "SNK-RUN-001-MSTR",
            MasterBarcode: "SNK-RUN-001-MSTR-BAR",
            Price: 89.99m,
            CompareAtPrice: 74.99m,
            Sizes: [("8", "SNK-RUN-001-8"), ("9", "SNK-RUN-001-9"), ("10", "SNK-RUN-001-10")]
        ), cancellationToken);

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task SeedProductWithVariants(ProductSeed seed, CancellationToken ct)
    {
        if (seed.Taxon is null)
            return;

        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var productResult = ProductMethod.Create(
            name: seed.Name,
            slug: seed.Slug,
            description: seed.Description,
            status: ProductStatus.Active,
            availableOn: DateTimeOffset.UtcNow,
            metaTitle: seed.MetaTitle,
            metaDescription: seed.Description,
            metaKeywords: seed.MetaKeywords,
            id: productId);

        var product = productResult.Value;
        product.GenderTarget = seed.Taxon.Name;

        var masterResult = VariantMethod.Create(
            productId: productId,
            sku: seed.MasterSku,
            isMaster: true,
            position: 0,
            barcode: seed.MasterBarcode,
            id: variantId);

        var masterVariant = masterResult.Value;
        masterVariant.Price = seed.Price;

        var masterPriceResult = PriceMethod.Create(
            amount: seed.Price,
            currency: "USD",
            variantId: variantId,
            compareAtAmount: seed.CompareAtPrice,
            countryIso: "US");

        var masterPrice = masterPriceResult.Value!;
        masterPrice.IsDefault = true;

        var classificationResult = ClassificationMethod.Create(
            productId: productId,
            taxonId: seed.Taxon.Id,
            position: 0);

        product.Variants.Add(masterVariant);
        product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);

        Context.Set<Product>().Add(product);
        Context.Set<Variant>().Add(masterVariant);
        Context.Set<Price>().Add(masterPrice);

        int pos = 1;
        foreach (var (size, sku) in seed.Sizes!)
        {
            var childVariantId = Guid.NewGuid();
            var childResult = VariantMethod.Create(
                productId: productId,
                sku: sku,
                isMaster: false,
                position: pos,
                barcode: $"{sku}-BAR",
                id: childVariantId);

            var childVariant = childResult.Value;
            childVariant.Price = seed.Price;

            var childPriceResult = PriceMethod.Create(
                amount: seed.Price,
                currency: "USD",
                variantId: childVariantId,
                compareAtAmount: seed.CompareAtPrice,
                countryIso: "US");

            var childPrice = childPriceResult.Value!;
            product.Variants.Add(childVariant);
            Context.Set<Variant>().Add(childVariant);
            Context.Set<Price>().Add(childPrice);
            pos++;
        }
    }

    private async Task SeedProductWithoutSizes(ProductSeed seed, CancellationToken ct)
    {
        if (seed.Taxon is null)
            return;

        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var productResult = ProductMethod.Create(
            name: seed.Name,
            slug: seed.Slug,
            description: seed.Description,
            status: ProductStatus.Active,
            availableOn: DateTimeOffset.UtcNow,
            metaTitle: seed.MetaTitle,
            metaDescription: seed.Description,
            metaKeywords: seed.MetaKeywords,
            id: productId);

        var product = productResult.Value;
        product.GenderTarget = "Unisex";

        var variantResult = VariantMethod.Create(
            productId: productId,
            sku: seed.MasterSku,
            isMaster: true,
            position: 0,
            barcode: seed.MasterBarcode,
            id: variantId);

        var variant = variantResult.Value;
        variant.Price = seed.Price;

        var priceResult = PriceMethod.Create(
            amount: seed.Price,
            currency: "USD",
            variantId: variantId,
            compareAtAmount: seed.CompareAtPrice,
            countryIso: "US");

        var price = priceResult.Value;
        price.IsDefault = true;

        var classificationResult = ClassificationMethod.Create(
            productId: productId,
            taxonId: seed.Taxon.Id,
            position: 0);

        product.Variants.Add(variant);
        product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);

        Context.Set<Product>().Add(product);
        Context.Set<Variant>().Add(variant);
        Context.Set<Price>().Add(price);
    }

    private sealed record ProductSeed(
        string Name,
        string Slug,
        string Description,
        string MetaTitle,
        string MetaKeywords,
        Taxon? Taxon,
        string MasterSku,
        string MasterBarcode,
        decimal Price,
        decimal? CompareAtPrice = null,
        (string Size, string Sku)[]? Sizes = null);
}