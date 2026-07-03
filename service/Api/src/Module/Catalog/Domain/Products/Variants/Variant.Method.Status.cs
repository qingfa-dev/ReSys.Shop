namespace Module.Catalog.Domain.Products.Variants;

public static partial class VariantMethod
{
    #region Status Methods
    public static bool IsPublished(this Variant variant)
    {
        return !variant.IsDeleted
            && (!variant.DiscontinuedOn.HasValue || variant.DiscontinuedOn > DateTimeOffset.UtcNow);
    }

    public static Result Publish(this Variant variant)
    {
        if (variant.IsDeleted)
        {
            return VariantResult.Errors.AlreadyDeleted;
        }

        if (variant.IsPublished())
        {
            return Result.Ok();
        }

        variant.DiscontinuedOn = null;
        return Result.Ok(VariantResult.Success.Updated(variant.Id));
    }

    public static Result Discontinue(this Variant variant)
    {
        if (variant.DiscontinuedOn <= DateTimeOffset.UtcNow)
        {
            return VariantResult.Errors.AlreadyDiscontinued;
        }

        variant.DiscontinuedOn = DateTimeOffset.UtcNow;

        return Result.Ok(VariantResult.Success.Discontinued);
    }
    #endregion
}
