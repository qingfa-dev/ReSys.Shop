namespace Module.Promotions.Domain.PromotionCategories;

public static class PromotionCategoryExtensions
{
    #region Factory Methods
    /// <summary>Creates a new promotion category for grouping promotions.</summary>
    /// <param name="name">The category name.</param>
    /// <param name="code">Optional unique code for the category.</param>
    /// <param name="presentation">Optional display name for the category.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A Result containing the created PromotionCategory on success.</returns>
    // Contract: pre=name is non-null and non-empty, post=entity.Id is not default, throws=none
    public static Result<PromotionCategory> Create(
        string name,
        string? code = null,
        string? presentation = null,
        Guid? id = null)
    {
        return new PromotionCategory
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Code = code,
            Presentation = presentation,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>Updates the category name, code, and presentation.</summary>
    /// <param name="category">The category to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="code">Optional new code.</param>
    /// <param name="presentation">Optional new presentation.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this PromotionCategory category,
        string? name = null,
        string? code = null,
        string? presentation = null)
    {
        category.Name = name ?? category.Name;
        category.Code = code ?? category.Code;
        category.Presentation = presentation ?? category.Presentation;

        return Result.Ok();
    }

    /// <summary>Soft-deletes the promotion category.</summary>
    /// <param name="category">The category to delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this PromotionCategory category, string deletedBy)
    {
        if (category.IsDeleted)
        {
            return Result.Ok();
        }

        category.IsDeleted = true;
        category.DeletedAtUtc = DateTimeOffset.UtcNow;
        category.DeletedBy = deletedBy;

        return Result.Ok();
    }
    #endregion Methods
}