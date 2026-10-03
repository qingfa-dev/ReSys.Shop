using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Mappings;

public static partial class TaxonMapping
{
    // Create:
    public static Result<Taxon> MapToDomain<T>(this T request, Guid taxonomyId) where T : TaxonRequest
    {
        return TaxonMethod.Create(
        #region Relationships
            taxonomyId,
            request.ParentId,
        #endregion
        #region Properties
            name: request.Name,
            presentation: request.Presentation,
            description: request.Description,
            descriptionHtml: null,
            position: request.Position,
        #endregion
        #region Settings
            automatic: request.Automatic,
            rulesMatchPolicy: request.RulesMatchPolicy,
            sortOrder: request.SortOrder,
            hideFromNav: request.HideFromNav,
        #endregion
        #region Images
            imageUrl: request.ImageUrl,
            squareImageUrl: request.SquareImageUrl,
        #endregion
        #region SEO
            slug: request.Slug,
            metaTitle: request.MetaTitle,
            metaDescription: request.MetaDescription,
            metaKeywords: request.MetaKeywords
        #endregion
        );
    }

    // Update:
    public static Result MapToDomain<T>(this T request, Taxon entity) where T : TaxonRequest
    {
        return entity.Update(
        #region Relationships
            parentId: request.ParentId,
        #endregion
        #region Properties
            name: request.Name,
            presentation: request.Presentation,
            description: request.Description,
            position: request.Position,
        #endregion
        #region Settings
            automatic: request.Automatic,
            rulesMatchPolicy: request.RulesMatchPolicy,
            sortOrder: request.SortOrder,
            hideFromNav: request.HideFromNav,
        #endregion
        #region Images
            imageUrl: request.ImageUrl,
            squareImageUrl: request.SquareImageUrl,
        #endregion
        #region SEO
            slug: request.Slug,
            metaTitle: request.MetaTitle,
            metaDescription: request.MetaDescription,
            metaKeywords: request.MetaKeywords
        #endregion
        );
    }
}

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
