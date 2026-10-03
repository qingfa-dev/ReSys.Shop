using Module.Catalog.Domain.Products;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Concerns.Sluggable;
using Shared.Application.Domain.Concerns.SoftDeletable;

using Slugify;

namespace Module.Catalog.Domain.Taxons;

public static class TaxonMethod
{
    private static readonly SlugHelper SlugHelper = new();

    #region Factory Methods
    /// <summary>
    /// Creates a new taxon within a taxonomy hierarchy.
    /// </summary>
    /// <param name="taxonomyId">The parent taxonomy identifier.</param>
    /// <param name="parentId">Optional parent taxon identifier for nested hierarchy.</param>
    /// <param name="name">The taxon name. Must not be null or empty.</param>
    /// <param name="presentation">The display presentation text.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="descriptionHtml">Optional HTML description.</param>
    /// <param name="position">Display order position.</param>
    /// <param name="slug">The URL slug. Must not be null or empty.</param>
    /// <param name="metaTitle">Optional SEO meta title.</param>
    /// <param name="metaDescription">Optional SEO meta description.</param>
    /// <param name="metaKeywords">Optional SEO meta keywords.</param>
    /// <param name="automatic">Whether products are classified automatically. Defaults to false.</param>
    /// <param name="rulesMatchPolicy">The match policy for automatic classification rules.</param>
    /// <param name="sortOrder">The default sort order for products in this taxon.</param>
    /// <param name="hideFromNav">Whether to hide from navigation. Defaults to false.</param>
    /// <param name="imageUrl">Optional image URL.</param>
    /// <param name="squareImageUrl">Optional square image URL.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>A Result containing the created Taxon.</returns>
    // Contract: pre=taxonomyId!=Guid.Empty&&name!=null&&slug!=null,
    //           post=entity.Id!=null&&entity.Name==name&&entity.Slug==slug, throws=ArgumentException
    public static Result<Taxon> Create(
    #region Relationships
        Guid taxonomyId,
        Guid? parentId,
    #endregion
    #region Properties
        string name,
        string? presentation,
        string? description,
        int position,
    #endregion
    #region Seo
        string slug,
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords,
    #endregion
    #region Settings
        bool automatic,
        TaxonMatchPolicy? rulesMatchPolicy,
        TaxonSortOrder? sortOrder,
        bool hideFromNav,
    #endregion
    #region Images
        string? imageUrl,
        string? squareImageUrl,
    #endregion
        string? descriptionHtml = null,
        Guid? id = null)
    {
        // Validate: TaxonomyId must be valid, Name and Slug must not be null or empty
        var taxon = new Taxon
        {
            Id = id ?? Guid.NewGuid(),

            #region Relationships
            TaxonomyId = taxonomyId,
            ParentId = parentId,
            #endregion

            #region Properties
            Name = name,
            Presentation = presentation,
            Description = description,
            DescriptionHtml = descriptionHtml,
            Position = position,
            #endregion

            #region SEO
            Slug = slug,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            #endregion

            #region Settings
            Automatic = automatic,
            RulesMatchPolicy = rulesMatchPolicy ?? TaxonMatchPolicy.All,
            SortOrder = sortOrder ?? TaxonSortOrder.Manual,
            HideFromNav = hideFromNav,
            #endregion

            #region Images
            ImageUrl = imageUrl,
            SquareImageUrl = squareImageUrl
            #endregion
        };

        ParameterizableBehavior.ApplyNormalization(taxon);
        if (string.IsNullOrWhiteSpace(slug))
        {
            SluggableBehavior.ApplySlugging(taxon, name);
        }

        if (taxon.Automatic)
        {
            taxon.MarkedForRegenerateTaxonProducts = true;
        }

        return taxon;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the taxon with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="taxon">The taxon to update.</param>
    /// <param name="parentId">Optional new parent identifier.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="presentation">Optional new presentation text.</param>
    /// <param name="description">Optional new description.</param>
    /// <param name="position">Optional new position.</param>
    /// <param name="slug">Optional new slug.</param>
    /// <param name="metaTitle">Optional new meta title.</param>
    /// <param name="metaDescription">Optional new meta description.</param>
    /// <param name="metaKeywords">Optional new meta keywords.</param>
    /// <param name="automatic">Optional new automatic flag.</param>
    /// <param name="rulesMatchPolicy">Optional new rules match policy.</param>
    /// <param name="sortOrder">Optional new sort order.</param>
    /// <param name="hideFromNav">Optional new hide-from-nav flag.</param>
    /// <param name="imageUrl">Optional new image URL.</param>
    /// <param name="squareImageUrl">Optional new square image URL.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this Taxon taxon,
    #region Relationships
        Guid? parentId,
    #endregion
    #region Properties
        string? name,
        string? presentation,
        string? description,
        int? position,
    #endregion
    #region Seo
        string? slug,
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords,
    #endregion
    #region Settings
        bool? automatic,
        TaxonMatchPolicy? rulesMatchPolicy,
        TaxonSortOrder? sortOrder,
        bool? hideFromNav,
    #endregion
    #region Images
        string? imageUrl,
        string? squareImageUrl
    #endregion
    )
    {
        var oldParentId = taxon.ParentId;
        var oldPosition = taxon.Position;

        var oldAutomatic = taxon.Automatic;
        var oldMatchPolicy = taxon.RulesMatchPolicy;

        #region Relationships
        taxon.ParentId = parentId ?? taxon.ParentId;
        #endregion

        #region Properties
        taxon.Name = name ?? taxon.Name;
        taxon.Presentation = presentation ?? taxon.Presentation;
        taxon.Description = description ?? taxon.Description;
        taxon.Position = position ?? taxon.Position;
        taxon.HideFromNav = hideFromNav ?? taxon.HideFromNav;
        #endregion

        #region SEO
        taxon.Slug = slug ?? taxon.Slug;
        taxon.MetaTitle = metaTitle ?? taxon.MetaTitle;
        taxon.MetaDescription = metaDescription ?? taxon.MetaDescription;
        taxon.MetaKeywords = metaKeywords ?? taxon.MetaKeywords;
        #endregion

        #region Settings
        taxon.Automatic = automatic ?? taxon.Automatic;
        taxon.RulesMatchPolicy = rulesMatchPolicy ?? taxon.RulesMatchPolicy;
        taxon.SortOrder = sortOrder ?? taxon.SortOrder;
        #endregion

        #region Images
        taxon.ImageUrl = imageUrl ?? taxon.ImageUrl;
        taxon.SquareImageUrl = squareImageUrl ?? taxon.SquareImageUrl;
        #endregion

        ParameterizableBehavior.ApplyNormalization(taxon);
        if (string.IsNullOrWhiteSpace(slug))
        {
            SluggableBehavior.ApplySlugging(taxon, taxon.Name);
        }

        // Assign: Record audit timestamp on modification
        AuditableBehavior.Touch(taxon);

        if (oldAutomatic != taxon.Automatic || oldMatchPolicy != taxon.RulesMatchPolicy)
        {
            taxon.MarkedForRegenerateTaxonProducts = true;
        }
        return Result.Ok();
    }

    /// <summary>
    /// Moves the taxon to a new parent at the specified position.
    /// </summary>
    /// <param name="taxon">The taxon to move.</param>
    /// <param name="newParentId">The new parent taxon identifier. Null for root.</param>
    /// <param name="newPosition">The new display position.</param>
    /// <param name="movedBy">Optional identifier of the user performing the move.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Move(this Taxon taxon, Guid? newParentId, int newPosition, string? movedBy = null)
    {
        if (taxon.ParentId == newParentId && taxon.Position == newPosition)
        {
            return Result.Ok();
        }

        var oldParentId = taxon.ParentId;
        taxon.ParentId = newParentId;
        taxon.Position = newPosition;

        // Assign: Record audit timestamp on modification
        AuditableBehavior.Touch(taxon);

        return Result.Ok();
    }

    /// <summary>
    /// Moves the taxon to be a child of the specified parent taxon.
    /// </summary>
    /// <param name="taxon">The taxon to move.</param>
    /// <param name="newParent">The new parent taxon.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result MoveToChildOf(this Taxon taxon, Taxon newParent)
    {
        taxon.ParentId = newParent.Id;
        taxon.Depth = newParent.Depth + 1;

        // Assign: Record audit timestamp on modification
        AuditableBehavior.Touch(taxon);

        return Result.Ok();
    }

    /// <summary>
    /// Moves the taxon to the root level (no parent).
    /// </summary>
    /// <param name="taxon">The taxon to move.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result MoveToRoot(this Taxon taxon)
    {
        taxon.ParentId = null;

        // Assign: Record audit timestamp on modification
        AuditableBehavior.Touch(taxon);

        return Result.Ok();
    }

    /// <summary>
    /// Soft-deletes the taxon.
    /// </summary>
    /// <param name="taxon">The taxon to delete.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this Taxon taxon)
    {
        SoftDeletableBehavior.Delete(taxon);

        return Result.Ok();
    }

    // Compute: Find all products classified under this taxon that are available
    public static ICollection<Product> ActiveProducts(this Taxon taxon, DateTimeOffset? date = null)
    {
        var checkDate = date ?? DateTimeOffset.UtcNow;
        // @CAT-5 Compute: Filter classifications to products available at the given date
        return taxon.Classifications
            .Where(c => c.Product != null && !c.Product.IsDeleted && c.Product.Status == ProductStatus.Active
                && (c.Product.AvailableOn == null || c.Product.AvailableOn <= checkDate))
            .Select(c => c.Product!)
            .ToList();
    }

    /// <summary>
    /// Restores a previously soft-deleted taxon.
    /// </summary>
    /// <param name="taxon">The taxon to restore.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Restore(this Taxon taxon)
    {
        SoftDeletableBehavior.Restore(taxon);

        return Result.Ok();
    }

    /// <summary>
    /// Updates the permalink based on the taxonomy name and parent hierarchy.
    /// </summary>
    /// <param name="taxon">The taxon to update.</param>
    /// <param name="taxonomyName">The parent taxonomy name for slug path generation.</param>
    // Compute: Build permalink from taxonomy name and parent hierarchy
    public static void UpdatePermalink(this Taxon taxon, string taxonomyName)
    {
        var taxonomySlug = SlugHelper.GenerateSlug(taxonomyName);
        var isPrimaryRoot = taxon.Parent == null && taxon.Slug.Equals(taxonomySlug, StringComparison.OrdinalIgnoreCase);

        var slugPath = taxon.Parent != null ? $"{taxon.Parent.Permalink}/{taxon.Slug}" : (isPrimaryRoot ? taxon.Slug : $"{taxonomySlug}/{taxon.Slug}");
        taxon.Permalink = slugPath?.Trim('/') ?? string.Empty;
    }

    /// <summary>
    /// Updates the pretty name based on the taxonomy name and parent hierarchy.
    /// </summary>
    /// <param name="taxon">The taxon to update.</param>
    /// <param name="taxonomyName">The parent taxonomy name for display path generation.</param>
    // Compute: Build pretty name from taxonomy name and parent hierarchy
    public static void UpdatePrettyName(this Taxon taxon, string taxonomyName)
    {
        var prettyPath = taxon.Parent != null ? $"{taxon.Parent.PrettyName} -> {taxon.Presentation}" : taxon.Presentation;
        taxon.PrettyName = prettyPath ?? string.Empty;
    }
    #endregion
}