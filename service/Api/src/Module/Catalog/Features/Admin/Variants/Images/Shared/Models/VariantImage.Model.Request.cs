namespace Module.Catalog.Features.Admin.Variants.Images.Shared.Models;

/// <summary>
/// Request payload for uploading a new variant image via multipart form.
/// </summary>
public record UploadImageRequest : VariantImageParameters
{
    /// <summary>Foreign key to the parent variant.</summary>
    public Guid VariantId { get; init; }
    /// <summary>The image file. Accepted formats: JPEG, PNG, GIF, WebP. Max size: 10 MB.</summary>
    public IFormFile File { get; init; } = null!;
}

/// <summary>
/// Request payload for updating an existing variant image's metadata.
/// </summary>
public record UpdateImageRequest : VariantImageParameters
{
}