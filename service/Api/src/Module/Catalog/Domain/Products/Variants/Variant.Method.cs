namespace Module.Catalog.Domain.Products.Variants;

public static partial class VariantMethod
{
    #region Factory Methods
    public static Result<Variant> Create(
        Guid productId,
        string sku,
        bool isMaster = false,
        int position = 0,
        string? barcode = null,
        string? hsCode = null,
        Guid? id = null)
    {
        var variant = new Variant
        {
            Id = id ?? Guid.NewGuid(),
            ProductId = productId,
            Sku = sku,
            IsMaster = isMaster,
            Position = position,
            Barcode = barcode,
            HsCode = hsCode
        };

        return variant;
    }
    #endregion

    #region Lifecycle Methods
    public static Result Update(this Variant variant,
        string? sku = null,
        int? position = null,
        bool? trackInventory = null,
        string? barcode = null,
        string? hsCode = null)
    {
        variant.Sku = sku ?? variant.Sku;
        variant.Position = position ?? variant.Position;
        variant.TrackInventory = trackInventory ?? variant.TrackInventory;
        variant.Barcode = barcode ?? variant.Barcode;
        variant.HsCode = hsCode ?? variant.HsCode;

        return Result.Ok();
    }

    public static Result Delete(this Variant variant, string deletedBy)
    {
        if (variant.IsDeleted)
        {
            return Result.Ok();
        }

        variant.IsDeleted = true;
        variant.DeletedAtUtc = DateTimeOffset.UtcNow;
        variant.DeletedBy = deletedBy;

        return Result.Ok();
    }
    #endregion
}