using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;


/// <summary>
/// Abstract base class for taxon-related parameters, providing common properties.
/// </summary>
public abstract record TaxonParameters
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