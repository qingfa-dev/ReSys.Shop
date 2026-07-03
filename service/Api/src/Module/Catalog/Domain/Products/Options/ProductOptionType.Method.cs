namespace Module.Catalog.Domain.Products.Options;

public static class ProductOptionTypeMethod
{
    /// <summary>
    /// Creates a new association between a product and an option type.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="optionTypeId">The option type identifier.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <returns>A Result containing the created ProductOptionType.</returns>
    // Contract: pre=productId!=Guid.Empty&&optionTypeId!=Guid.Empty,
    //           post=entity.ProductId==productId&&entity.OptionTypeId==optionTypeId, throws=ArgumentException
    public static Result<ProductOptionType> Create(Guid productId, Guid optionTypeId, int position = 0)
    {
        return new ProductOptionType
        {
            ProductId = productId,
            OptionTypeId = optionTypeId,
            Position = position
        };
    }
}