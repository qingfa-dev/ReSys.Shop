using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Ordering.Features.Storefront.Shared.Mappings;

namespace Module.Ordering.Features.Storefront.Shared.Services;

/// <summary>Builds the variant → product reference lookup (sku, product id, name, primary image) used to enrich cart and order line-item responses.</summary>
public static class ProductLookupFactory
{
    /// <summary>Loads the parent products and primary images for the given variant ids.</summary>
    public static async Task<IReadOnlyDictionary<Guid, CartItemLookup>> BuildAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<Guid> variantIds,
        CancellationToken cancellationToken)
    {
        if (variantIds.Count == 0)
            return new Dictionary<Guid, CartItemLookup>();

        var variants = await dbContext.Set<Variant>()
            .Where(v => variantIds.Contains(v.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
        var products = await dbContext.Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .Include(p => p.Variants)
                .ThenInclude(v => v.VariantImages)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productsById = products.ToDictionary(p => p.Id);

        return variants.ToDictionary(v => v.Id, v =>
        {
            if (!productsById.TryGetValue(v.ProductId, out var product))
                return new CartItemLookup { Sku = v.Sku ?? string.Empty, ProductId = v.ProductId };

            // Primary image: master variant's first image by position, falling back to the first image across all variants.
            var masterVariant = product.Variants.FirstOrDefault(x => x.IsMaster);
            var primaryImageUrl = (masterVariant?.VariantImages.OrderBy(i => i.Position).FirstOrDefault()
                ?? product.Variants.SelectMany(x => x.VariantImages).OrderBy(i => i.Position).FirstOrDefault())
                ?.Url;

            return new CartItemLookup
            {
                Sku = v.Sku ?? string.Empty,
                ProductId = v.ProductId,
                ProductName = product.Name,
                ProductImageUrl = primaryImageUrl,
            };
        });
    }
}
