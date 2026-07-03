namespace Module.Catalog.Domain.Products.Classifications;

public static class ClassificationMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new classification (product-taxon association).
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="taxonId">The taxon identifier.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <param name="isAutomatic">Whether the classification was created automatically. Defaults to false.</param>
    /// <returns>A Result containing the created Classification.</returns>
    // Contract: pre=productId!=null&&taxonId!=null,
    //           post=entity.ProductId==productId&&entity.TaxonId==taxonId, throws=ArgumentException
    public static Result<Classification> Create(Guid? productId, Guid? taxonId, int position = 0, bool isAutomatic = false)
    {
        return new Classification
        {
            ProductId = productId,
            TaxonId = taxonId,
            Position = position,
            IsAutomatic = isAutomatic
        };
    }
    #endregion
}