using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Mappings;

// Boundary: Features → Domain — maps Order entities to cart response DTOs
public static partial class CartMapping
{
    public static T EmptyCart<T>() where T : CartDetailResponse, new()
    {
        return new T();
    }


    public static T MapToDetail<T>(this Order entity) where T : CartDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            ItemTotal = entity.ItemTotal,
            Total = entity.Total,
            Currency = entity.Currency,
            ItemCount = entity.ItemCount,
            CheckoutState = entity.CheckoutState.ToString(),
        };
    }

    public static CartItem MapToCartItem(this LineItem lineItem, CartItemLookup lookup)
    {
        return new CartItem
        {
            Id = lineItem.Id,
            VariantId = lineItem.VariantId,
            VariantName = lookup.Sku,
            Sku = lookup.Sku,
            ProductName = lookup.ProductName,
            ProductImageUrl = lookup.ProductImageUrl,
            Quantity = lineItem.Quantity,
            Price = lineItem.Price,
            Total = lineItem.Total,
        };
    }

    public static T MapToDetailWithItems<T>(this Order entity, IReadOnlyDictionary<Guid, CartItemLookup> itemLookup)
        where T : CartDetailResponse, new()
    {
        var result = entity.MapToDetail<T>();
        result = result with
        {
            Items = entity.LineItems.Select(li =>
            {
                itemLookup.TryGetValue(li.VariantId, out var lookup);
                return li.MapToCartItem(lookup ?? new CartItemLookup());
            }).ToList()
        };
        return result;
    }

    /// <summary>Legacy overload: sku-only enrichment for cart flows that have not yet been extended with product lookup.</summary>
    public static T MapToDetailWithItems<T>(this Order entity, Dictionary<Guid, string> variantNames)
        where T : CartDetailResponse, new()
    {
        var itemLookup = variantNames.ToDictionary(
            kv => kv.Key,
            kv => new CartItemLookup { Sku = kv.Value });
        return entity.MapToDetailWithItems<T>(itemLookup);
    }

    /// <summary>Builds the enriched cart item lookup (sku, product name, primary image) for the given variant ids.</summary>
    public static async Task<Dictionary<Guid, CartItemLookup>> BuildCartItemLookupAsync(
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
                return new CartItemLookup { Sku = v.Sku ?? string.Empty };

            // Primary image: master variant's first image by position, falling back to the first image across all variants.
            var masterVariant = product.Variants.FirstOrDefault(x => x.IsMaster);
            var primaryImageUrl = (masterVariant?.VariantImages.OrderBy(i => i.Position).FirstOrDefault()
                ?? product.Variants.SelectMany(x => x.VariantImages).OrderBy(i => i.Position).FirstOrDefault())
                ?.Url;

            return new CartItemLookup
            {
                Sku = v.Sku ?? string.Empty,
                ProductName = product.Name,
                ProductImageUrl = primaryImageUrl,
            };
        });
    }
}

/// <summary>Enrichment data for a single cart line item, used to render the storefront cart.</summary>
public sealed record CartItemLookup
{
    /// <summary>Variant SKU (also used as the variant display name).</summary>
    public string Sku { get; init; } = string.Empty;
    /// <summary>Display name of the parent product.</summary>
    public string? ProductName { get; init; }
    /// <summary>Primary image URL of the product.</summary>
    public string? ProductImageUrl { get; init; }
}