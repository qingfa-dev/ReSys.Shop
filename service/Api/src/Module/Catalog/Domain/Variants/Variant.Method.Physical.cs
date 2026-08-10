namespace Module.Catalog.Domain.Variants;

public static partial class VariantMethod
{
    #region Physical Specifications
    public static Result UpdatePhysicalSpecs(this Variant variant,
        decimal? weight = null,
        WeightUnit? weightUnit = null,
        decimal? height = null,
        decimal? width = null,
        decimal? depth = null,
        DimensionUnit? dimensionsUnit = null)
    {
        variant.Weight = weight ?? variant.Weight;
        variant.WeightUnit = weightUnit ?? variant.WeightUnit;
        variant.Height = height ?? variant.Height;
        variant.Width = width ?? variant.Width;
        variant.Depth = depth ?? variant.Depth;
        variant.DimensionsUnit = dimensionsUnit ?? variant.DimensionsUnit;

        return Result.Ok();
    }
    #endregion
}