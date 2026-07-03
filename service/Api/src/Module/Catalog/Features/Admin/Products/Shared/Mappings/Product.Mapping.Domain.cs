using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Shared.Mappings;

/// <summary>
/// Maps between ProductRequest DTOs and Product domain entities.
/// </summary>
public static partial class ProductMapping
{
    /// <summary>
    /// Maps a product request to a new Product domain entity via factory Create.
    /// </summary>
    /// <typeparam name="T">The request type deriving from <see cref="ProductRequest"/>.</typeparam>
    /// <param name="request">The request payload with product attributes.</param>
    /// <returns>A result containing the new Product entity or validation failures.</returns>
    public static Result<Product> MapToDomain<T>(this T request) where T : ProductRequest
    {
        return ProductMethod.Create(
            name: request.Name,
            slug: request.Slug,
            description: request.Description,
            metaTitle: request.MetaTitle,
            metaDescription: request.MetaDescription,
            metaKeywords: request.MetaKeywords,
            availableOn: request.AvailableOn,
            discontinueOn: request.DiscontinueOn,
            taxCategoryId: request.TaxCategoryId);
    }

    /// <summary>
    /// Applies a product request fields to an existing Product entity via domain Update.
    /// </summary>
    /// <typeparam name="T">The request type deriving from <see cref="ProductRequest"/>.</typeparam>
    /// <param name="request">The request payload with updated product attributes.</param>
    /// <param name="product">The existing product entity to update.</param>
    /// <returns>A result indicating success or validation failures.</returns>
    public static Result MapToDomain<T>(this T request, Product product) where T : ProductRequest
    {
        return product.Update(
            name: request.Name,
            slug: request.Slug,
            description: request.Description,
            metaTitle: request.MetaTitle,
            metaDescription: request.MetaDescription,
            metaKeywords: request.MetaKeywords,
            availableOn: request.AvailableOn,
            discontinueOn: request.DiscontinueOn,
            taxCategoryId: request.TaxCategoryId);
    }
}
