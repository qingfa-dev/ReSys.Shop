using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxons.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Shared.Mappings;

public static partial class TaxonMapping
{
    // List Item:
    public static T MapToListItem<T>(this Taxon entity) where T : TaxonListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            #region Relationships
            ParentId = entity.ParentId,
            ParentName = entity.Parent?.Name,
            TaxonomyId = entity.TaxonomyId,
            TaxonomyName = entity.Taxonomy?.Name,
            #endregion
            #region Properties
            Name = entity.Name,
            Presentation = entity.Presentation,
            Description = entity.Description,
            Position = entity.Position,
            #endregion
            #region SEO
            Slug = entity.Slug,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            #endregion
            #region Images
            ImageUrl = entity.ImageUrl,
            SquareImageUrl = entity.SquareImageUrl,
            #endregion
            #region Settings
            Automatic = entity.Automatic,
            RulesMatchPolicy = entity.RulesMatchPolicy,
            SortOrder = entity.SortOrder,
            HideFromNav = entity.HideFromNav,
            #endregion
            #region Nested Set
            Lft = entity.Lft,
            Rgt = entity.Rgt,
            Depth = entity.Depth,
            #endregion
            #region Stats
            TaxonRuleCount = entity.TaxonRules?.Count ?? 0,
            ProductCount = entity.Classifications?.Count ?? 0,
            ChildrenCount = entity.Children?.Count ?? 0,
            #endregion
            #region Automatic
            Permalink = entity.Permalink,
            PrettyName = entity.PrettyName,
            #endregion
            #region Auditable
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc
            #endregion
        };
    }

    // Detail:
    public static T MapToDetail<T>(this Taxon entity) where T : TaxonDetailResponse, new()
    {

        return new T
        {
            Id = entity.Id,
            #region Relationships
            ParentId = entity.ParentId,
            ParentName = entity.Parent?.Name,
            TaxonomyId = entity.TaxonomyId,
            TaxonomyName = entity.Taxonomy?.Name,
            #endregion
            #region Properties
            Name = entity.Name,
            Presentation = entity.Presentation,
            Description = entity.Description,
            Position = entity.Position,
            #endregion
            #region SEO
            Slug = entity.Slug,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            #endregion
            #region Images
            ImageUrl = entity.ImageUrl,
            SquareImageUrl = entity.SquareImageUrl,
            #endregion
            #region Settings
            Automatic = entity.Automatic,
            RulesMatchPolicy = entity.RulesMatchPolicy,
            SortOrder = entity.SortOrder,
            HideFromNav = entity.HideFromNav,
            #endregion
            #region Nested Set
            Lft = entity.Lft,
            Rgt = entity.Rgt,
            Depth = entity.Depth,
            #endregion
            #region Stats
            TaxonRuleCount = entity.TaxonRules.Count,
            ProductCount = entity.Classifications.Count,
            ChildrenCount = entity.Children.Count,
            #endregion
            #region Automatic
            Permalink = entity.Permalink,
            PrettyName = entity.PrettyName,
            #endregion
            #region Auditable
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc
            #endregion
        };
    }

    // Tree Item:
    public static T MapToTreeItem<T>(this Taxon entity) where T : TaxonTreeItem, new()
    {
        return new T
        {
            Id = entity.Id,
            #region Relationships
            ParentId = entity.ParentId,
            #endregion
            #region Properties
            Name = entity.Name,
            Presentation = entity.Presentation,
            Description = entity.Description,
            Position = entity.Position,
            #endregion
            #region SEO
            Slug = entity.Slug,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            #endregion
            #region Images
            ImageUrl = entity.ImageUrl,
            SquareImageUrl = entity.SquareImageUrl,
            #endregion
            #region Settings
            Automatic = entity.Automatic,
            RulesMatchPolicy = entity.RulesMatchPolicy,
            SortOrder = entity.SortOrder,
            HideFromNav = entity.HideFromNav,
            #endregion
            #region Nested Set
            Lft = entity.Lft,
            Rgt = entity.Rgt,
            Depth = entity.Depth,
            #endregion
            #region Stats
            TaxonRuleCount = entity.TaxonRules.Count,
            ProductCount = entity.Classifications.Count,
            ChildrenCount = entity.Children.Count,
            #endregion
            #region Automatic
            Permalink = entity.Permalink,
            PrettyName = entity.PrettyName,
            #endregion
            #region Auditable
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            #endregion
            #region Tree
            Children = entity.Children.Select(c => c.MapToTreeItem<T>()).ToList()
            #endregion
        };
    }
}