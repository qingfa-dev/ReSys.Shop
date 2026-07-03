namespace Module.Catalog.Domain.Products.Variants.Options;

public static class OptionValueVariantMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new association between a variant and an option value.
    /// </summary>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="optionValueId">The option value identifier.</param>
    /// <returns>A Result containing the created OptionValueVariant.</returns>
    // Contract: pre=variantId!=Guid.Empty&&optionValueId!=Guid.Empty,
    //           post=entity.VariantId==variantId&&entity.OptionValueId==optionValueId, throws=ArgumentException
    public static Result<OptionValueVariant> Create(Guid variantId, Guid optionValueId)
    {
        return new OptionValueVariant
        {
            VariantId = variantId,
            OptionValueId = optionValueId
        };
    }
    #endregion
}