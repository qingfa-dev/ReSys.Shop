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

    #region Business Rules
    /// <summary>
    /// Validates that the requested option values do not include more than one value
    /// from the same option type, combined with the option types already assigned to
    /// the variant. A variant must have exactly one value per option type.
    /// </summary>
    /// <param name="requestedOptionTypeIds">Option type ID for each requested option value ID.</param>
    /// <param name="existingOptionTypeIds">Option type IDs already assigned to the variant.</param>
    /// <returns>Failure with <see cref="OptionValueVariantResult.Errors.MultipleValuesPerOptionType"/>
    /// when the same option type appears more than once across the combined set; otherwise success.</returns>
    // Contract: pre=requestedOptionTypeIds!=null&&existingOptionTypeIds!=null, post=result!=null
    public static Result ValidateSingleValuePerOptionType(
        IReadOnlyCollection<Guid> requestedOptionTypeIds,
        IReadOnlyCollection<Guid> existingOptionTypeIds)
    {
        HashSet<Guid> seen = new(existingOptionTypeIds);

        foreach (Guid optionTypeId in requestedOptionTypeIds)
        {
            if (!seen.Add(optionTypeId))
                return OptionValueVariantResult.Errors.MultipleValuesPerOptionType;
        }

        return Result.Ok();
    }
    #endregion
}