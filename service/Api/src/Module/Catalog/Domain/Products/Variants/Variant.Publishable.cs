namespace Module.Catalog.Domain.Products.Variants;

public static class VariantPublishableExtensions
{
    // Compute: Determine if the variant is currently published and available
    public static bool IsPublished(this Variant variant)
    {
        return !variant.IsDeleted
            && (!variant.DiscontinuedOn.HasValue || variant.DiscontinuedOn > DateTimeOffset.UtcNow);
    }

    // Contract: pre=variant!=null, post=variant.DiscontinuedOn==null
    public static Result Publish(this Variant variant)
    {
        if (variant.IsDeleted)
        {
            return VariantResult.Errors.AlreadyDeleted;
        }

        variant.DiscontinuedOn = null;
        return Result.Ok(VariantResult.Success.Updated(variant.Id));
    }

    // Contract: pre=variant!=null, post=variant.DiscontinuedOn!=null
    public static Result Unpublish(this Variant variant)
    {
        if (variant.IsDeleted)
        {
            return VariantResult.Errors.AlreadyDeleted;
        }

        variant.DiscontinuedOn = DateTimeOffset.UtcNow;
        return Result.Ok(VariantResult.Success.Discontinued);
    }
}
