using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Admin.Taxons.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxons.Shared.Mappings;

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