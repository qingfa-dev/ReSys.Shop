namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Status Methods
    public static Result Activate(this Product product)
    {
        if (product.Status == ProductStatus.Active)
        {
            return ProductResult.Errors.AlreadyActive;
        }

        if (product.Status == ProductStatus.Archived)
            return ProductResult.Errors.CannotActivateArchivedProduct;

        product.Status = ProductStatus.Active;

        return Result.Ok(ProductResult.Success.Activated);
    }

    public static Result Archive(this Product product)
    {
        if (product.Status == ProductStatus.Archived)
        {
            return ProductResult.Errors.AlreadyArchived;
        }

        product.DiscontinueOn = DateTimeOffset.UtcNow;
        product.Status = ProductStatus.Archived;

        return Result.Ok(ProductResult.Success.Archived);
    }

    public static Result Draft(this Product product)
    {
        if (product.Status == ProductStatus.Draft)
        {
            return ProductResult.Errors.AlreadyDraft;
        }

        product.Status = ProductStatus.Draft;

        return Result.Ok(ProductResult.Success.Drafted);
    }

    public static Result Discontinue(this Product product)
    {
        if (product.DiscontinueOn <= DateTimeOffset.UtcNow)
        {
            return ProductResult.Errors.AlreadyDiscontinued;
        }

        product.DiscontinueOn = DateTimeOffset.UtcNow;
        product.Status = ProductStatus.Archived;

        return Result.Ok(ProductResult.Success.Discontinued);
    }

    public static Result ChangeStatus(this Product product, ProductStatus newStatus)
    {
        if (product.Status == newStatus)
        {
            return Result.Ok();
        }

        product.Status = newStatus;

        return Result.Ok();
    }
    #endregion
}
