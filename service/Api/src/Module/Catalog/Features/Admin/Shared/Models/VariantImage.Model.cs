using Module.Catalog.Domain.Variants.Images;

namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record VariantImageParameters
{
    public string? Alt { get; init; } = null;
    public int Position { get; init; } = 0;
    public VariantImageType Type { get; init; }
}

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
    /// <summary>
    /// Explicit image type. Null means "not provided" and the existing type is preserved on update.
    /// </summary>
    public new VariantImageType? Type { get; init; }
}

/// <summary>
/// Detailed variant image response returned by get-by-id and list endpoints.
/// </summary>
public record VariantImageDetailResponse : VariantImageParameters
{
    /// <summary>Unique identifier of the variant image.</summary>
    public Guid Id { get; init; }
    /// <summary>Foreign key to the parent variant.</summary>
    public Guid? VariantId { get; init; }
    /// <summary>Publicly accessible URL of the image.</summary>
    public string Url { get; init; } = string.Empty;
    /// <summary>MIME type of the image file.</summary>
    public string ContentType { get; init; } = string.Empty;
    /// <summary>Original filename at time of upload.</summary>
    public string FileName { get; init; } = string.Empty;
    /// <summary>File size in bytes.</summary>
    public int FileSize { get; init; }
    /// <summary>Image width in pixels (if available).</summary>
    public int? Width { get; init; }
    /// <summary>Image height in pixels (if available).</summary>
    public int? Height { get; init; }
    /// <summary>Unit of measurement for dimensions.</summary>
    public string? DimensionsUnit { get; init; }
    /// <summary>Timestamp when the image was uploaded (UTC).</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
}

/// <summary>
/// Variant image response returned by the download endpoint.
/// Extends the detail response with the binary stream.
/// </summary>
public record VariantImageDownloadResponse : VariantImageDetailResponse
{
    /// <summary>Binary stream of the image file for direct download.</summary>
    public Stream Stream { get; init; } = null!;
}
