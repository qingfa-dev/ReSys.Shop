namespace Module.Catalog.Features.Admin.Taxons.Shared.Models;

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

