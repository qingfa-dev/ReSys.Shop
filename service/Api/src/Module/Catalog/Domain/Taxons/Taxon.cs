using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxons.Rules;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.Sluggable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Taxons;

/// <summary>
/// Represents a node within a taxonomy tree used to categorize and filter products.
/// </summary>
// Invariant: Name != null; Slug != null; TaxonomyId != Guid.Empty; Lft < Rgt; Depth >= 0
public sealed partial class Taxon : Entity,
    ISluggable,
    IParameterizable,
    IAuditable,
    ISoftDeletable
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? Presentation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DescriptionHtml { get; set; }
    public int Position { get; set; }
    public int ChildrenCount { get; set; }
    #endregion Properties

    #region Auditable
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditable

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Nested Set
    public int Lft { get; set; }
    public int Rgt { get; set; }
    public int Depth { get; set; }
    #endregion Nested Set

    #region Settings
    public bool Automatic { get; set; }
    public TaxonMatchPolicy RulesMatchPolicy { get; set; } = TaxonMatchPolicy.All;
    public TaxonSortOrder SortOrder { get; set; } = TaxonSortOrder.Manual;
    public bool HideFromNav { get; set; }
    #endregion Settings

    #region Automatic
    public bool MarkedForRegenerateTaxonProducts { get; set; }
    public string Permalink { get; set; } = string.Empty;
    public string PrettyName { get; set; } = string.Empty;
    #endregion Automatic

    #region Images
    public string? ImageUrl { get; set; }
    public string? SquareImageUrl { get; set; }
    #endregion Images

    #region SEO
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    #endregion SEO

    #region Relationships
    public Guid TaxonomyId { get; set; }
    public Taxonomy Taxonomy { get; set; } = null!;
    public Guid? ParentId { get; set; }

    public Taxon? Parent { get; set; }
    public ICollection<Taxon> Children { get; set; } = new List<Taxon>();
    public ICollection<TaxonRule> TaxonRules { get; set; } = new List<TaxonRule>();
    public ICollection<Classification> Classifications { get; set; } = [];
    #endregion Relationships

    #region Computed
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsRoot => ParentId == null;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsLeaf => Children.Count == 0;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsChild => ParentId != null;
    #endregion Computed

    #region Constructor
    internal Taxon() { }
    #endregion Constructor
}