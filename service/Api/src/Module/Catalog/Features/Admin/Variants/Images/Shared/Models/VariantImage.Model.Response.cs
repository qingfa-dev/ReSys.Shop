namespace Module.Catalog.Features.Admin.Variants.Images.Shared.Models;

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