namespace Module.Catalog.Domain.Variants.Images;

public static class VariantImageMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new variant image record.
    /// </summary>
    /// <param name="contentType">The MIME type of the image file. Must not be null or empty.</param>
    /// <param name="fileName">The original filename at time of upload. Must not be null or empty.</param>
    /// <param name="fileSize">The file size in bytes. Must be greater than zero.</param>
    /// <param name="url">The publicly accessible URL for the image. Must not be null or empty.</param>
    /// <param name="storagePath">The storage provider path for delete and download operations. Must not be null or empty.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <param name="alt">Optional alternative text for accessibility and SEO.</param>
    /// <param name="type">Image usage classification. Defaults to Default.</param>
    /// <param name="variantId">Optional parent variant identifier.</param>
    /// <returns>A Result containing the created VariantImage.</returns>
    // Contract: pre=contentType!=null&&fileName!=null&&fileSize>0&&url!=null&&storagePath!=null,
    //           post=entity.Id!=null&&entity.Url==url, throws=ArgumentException
    public static Result<VariantImage> Create(
        string contentType,
        string fileName,
        int fileSize,
        string url,
        string storagePath,
        int position = 0,
        string? alt = null,
        VariantImageType type = VariantImageType.Default,
        Guid? variantId = null)
    {
        return new VariantImage
        {
            Id = Guid.NewGuid(),
            ContentType = contentType,
            FileName = fileName,
            FileSize = fileSize,
            Url = url,
            StoragePath = storagePath,
            Position = position,
            Alt = alt,
            Type = type,
            VariantId = variantId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the display details (position, alt text, type) for the image.
    /// </summary>
    /// <param name="image">The variant image to update.</param>
    /// <param name="position">Optional new display position.</param>
    /// <param name="alt">Optional new alt text.</param>
    /// <param name="type">Optional new image type.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result UpdateDetails(this VariantImage image,
        int? position = null,
        string? alt = null,
        VariantImageType? type = null)
    {
        image.Position = position ?? image.Position;
        image.Alt = alt ?? image.Alt;
        image.Type = type ?? image.Type;

        return Result.Ok();
    }
    #endregion
}