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
        {
            return Result.Ok();
        }

        var menTaxon = await Context.Set<Taxon>()
            .FirstOrDefaultAsync(t => t.Slug == "men", cancellationToken);

        if (menTaxon is null)
        {
            return Result.Ok();
        }

        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var productResult = ProductMethod.Create(
            name: "Classic Cotton T-Shirt",
            slug: "classic-cotton-t-shirt",
            description: "A comfortable classic cotton t-shirt perfect for everyday wear.",
            status: ProductStatus.Active,
            availableOn: DateTimeOffset.UtcNow,
            metaTitle: "Classic Cotton T-Shirt",
            metaDescription: "Shop our Classic Cotton T-Shirt. Comfortable, durable, and perfect for everyday wear.",
            metaKeywords: "t-shirt, cotton, classic, casual",
            id: productId);

        var product = productResult.Value;

        var variantResult = VariantMethod.Create(
            productId: productId,
            sku: "TEE-COTTON-001",
            isMaster: true,
            position: 0,
            barcode: "TEE-COTTON-001-BAR",
            id: variantId);

        var variant = variantResult.Value;
        variant.Price = 29.99m;

        var priceResult = PriceMethod.Create(
            amount: 29.99m,
            currency: "USD",
            variantId: variantId,
            compareAtAmount: null,
            countryIso: "US");

        var price = priceResult.Value;
        price.IsDefault = true;

        var classificationResult = ClassificationMethod.Create(
            productId: productId,
            taxonId: menTaxon.Id,
            position: 0);

        product.Variants.Add(variant);
        product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);

        Context.Set<Product>().Add(product);
        Context.Set<Variant>().Add(variant);
        Context.Set<Price>().Add(price);

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
