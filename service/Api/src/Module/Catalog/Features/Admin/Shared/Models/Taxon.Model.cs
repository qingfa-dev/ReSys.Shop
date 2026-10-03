using Module.Catalog.Domain.Taxons;

namespace Module.Catalog.Features.Admin.Shared.Models;

/// <summary>
/// Abstract base class for taxon-related parameters, providing common properties.
/// </summary>
public abstract record TaxonParameters : INamedParameters, ISortableParameters, ISeoParameters
{
    #region Relationships
    public Guid? ParentId { get; init; }
    public Guid TaxonomyId { get; init; }
    #endregion

    #region Properties
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Position { get; init; }
    #endregion

    #region SEO
    public string Slug { get; init; } = string.Empty;
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    #endregion

    #region Images
    public string? ImageUrl { get; init; }
    public string? SquareImageUrl { get; init; }
    #endregion

    #region Settings
    public bool Automatic { get; set; }
    public TaxonMatchPolicy RulesMatchPolicy { get; set; } = TaxonMatchPolicy.All;
    public TaxonSortOrder SortOrder { get; set; } = TaxonSortOrder.Manual;
    public bool HideFromNav { get; set; }
    #endregion
}

/// <summary>
/// Represents a request for taxon creation or update operations, including settings.
/// </summary>
public record TaxonRequest : TaxonParameters;

// List Item:
public record TaxonListItemResponse : TaxonParameters
{
    public Guid Id { get; set; }

    #region Relationships
    public string? ParentName { get; init; }
    public string? TaxonomyName { get; init; }
    #endregion

    #region Nested Set
    public int Lft { get; set; }
    public int Rgt { get; set; }
    public int Depth { get; set; }
    #endregion

    #region Stats
    public int? TaxonRuleCount { get; set; }
    public int? ProductCount { get; set; }
    public int? ChildrenCount { get; set; }
    #endregion

    #region  Automatic
    public string Permalink { get; set; } = string.Empty;
    public string PrettyName { get; set; } = string.Empty;
    #endregion


    #region Auditable
    public DateTimeOffset? CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    #endregion
}

// Detail:
public record TaxonDetailResponse : TaxonListItemResponse;

// Tree Item
public record TaxonTreeItem : TaxonListItemResponse
{
    public bool IsExpanded { get; set; }
    public bool IsInActivePath { get; set; }

    public IEnumerable<TaxonTreeItem> Children { get; set; } = [];
}
