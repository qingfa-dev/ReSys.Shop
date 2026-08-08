using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products.Variants.Images;

/// <summary>
/// Represents an image file associated with a product variant.
/// Stores metadata (dimensions, format, size) and the storage location.
/// </summary>
// Invariant: ContentType != null; FileName != null; Url != null; StoragePath != null; FileSize >= 0
public sealed partial class VariantImage : Entity<Guid>, IAuditable
{
    #region Properties
    /// <summary>MIME type of the image file (e.g., "image/jpeg").</summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>Original filename at time of upload.</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>File size in bytes.</summary>
    public int FileSize { get; set; }
    /// <summary>Image width in pixels (if available).</summary>
    public int? Width { get; set; }
    /// <summary>Image height in pixels (if available).</summary>
    public int? Height { get; set; }
    /// <summary>Unit of measurement for dimensions (px, in, cm).</summary>
    public string? DimensionsUnit { get; set; }
    /// <summary>Display order position (0-based ascending).</summary>
    public int Position { get; set; }
    /// <summary>Publicly accessible URL for the image.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>Storage provider path used for delete and download operations.</summary>
    public string StoragePath { get; set; } = string.Empty;
    /// <summary>Alternative text for accessibility and SEO.</summary>
    public string? Alt { get; set; }
    /// <summary>Image usage classification (Default, Thumbnail, Square, Gallery, Search).</summary>
    public VariantImageType Type { get; set; } = VariantImageType.Default;
    #endregion Properties

    #region Auditing
    /// <summary>Timestamp when the image was uploaded (UTC).</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Timestamp of the last modification (UTC).</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    /// <summary>Identifier of the user who uploaded the image.</summary>
    public string? CreatedBy { get; set; }
    /// <summary>Identifier of the user who last modified the image.</summary>
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Relationships
    /// <summary>Foreign key to the parent variant.</summary>
    public Guid? VariantId { get; set; }
    /// <summary>Navigation property to the parent variant.</summary>
    public Variant? Variant { get; set; }
    /// <summary>AI-generated image embeddings for semantic search.</summary>
    public ICollection<ImageEmbedding> ImageEmbeddings { get; set; } = new List<ImageEmbedding>();
    #endregion Relationships

    #region Constructor
    internal VariantImage() { }
    #endregion Constructor
}