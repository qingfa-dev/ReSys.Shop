using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Domain.Variants;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
// @CAT-10 Invariant: Status is Draft|Active|Archived; Slug unique; At least one OptionType if HasVariants; MasterVariant exists; AvailableOn <= DiscontinueOn when both set
public sealed partial class Product : Entity, IAuditable, ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    #endregion Properties

    #region SEO
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    #endregion SEO

    #region Timestamp
    public DateTimeOffset? AvailableOn { get; set; }
    public DateTimeOffset? DiscontinueOn { get; set; }
    public DateTimeOffset? MakeActiveAt { get; set; }
    #endregion Timestamp

    #region Fashion
    public string? StyleCode { get; set; }
    public string? SeasonName { get; set; }
    public string? MaterialComposition { get; set; }
    public string? CareInstructions { get; set; }
    public string? FitNotes { get; set; }
    public string? Department { get; set; }
    public string? GenderTarget { get; set; }
    #endregion Fashion

    #region Relationships
    public Guid MasterVariantId { get; set; }
    public ICollection<Variant> Variants { get; set; } = [];
    public ICollection<ProductOptionType> ProductOptionTypes { get; set; } = [];
    public ICollection<Classification> Classifications { get; set; } = [];
    public bool MarkedForRegenerateTaxonProducts { get; set; }
    #endregion Relationships

    #region SoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion SoftDeletable

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal Product() { } // For EF Core
    #endregion Constructor
}