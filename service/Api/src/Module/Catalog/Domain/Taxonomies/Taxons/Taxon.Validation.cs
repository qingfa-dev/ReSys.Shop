namespace Module.Catalog.Domain.Taxonomies.Taxons;

public static class TaxonValidation
{
    // Validate: Taxonomy ID must be non-empty
    public static IRuleBuilderOptions<T, Guid> ApplyTaxonomyIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        // Taxonomy ID: required, must be a valid GUID (handled by model binding)
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonResult.Errors.InvalidTaxonomyId.Code)
            .WithMessage(TaxonResult.Errors.InvalidTaxonomyId.Message);
    }

    public static IRuleBuilderOptions<T, Guid?> ApplyTaxonParentIdRules<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        // Taxon ID: optional, but if provided must be a valid GUID (handled by model binding)
        return ruleBuilder
            .Must(id => id == null || id != Guid.Empty)
            .WithErrorCode(TaxonResult.Errors.InvalidParentId.Code)
            .WithMessage(TaxonResult.Errors.InvalidParentId.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Name: required, max length
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonResult.Errors.NameRequired.Code)
            .WithMessage(TaxonResult.Errors.NameRequired.Message)
            .MaximumLength(TaxonConstant.Constraints.NameMaxLength)
            .WithErrorCode(TaxonResult.Errors.NameTooLong.Code)
            .WithMessage(TaxonResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Presentation: optional, max length
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(TaxonResult.Errors.PresentationTooLong.Code)
            .WithMessage(TaxonResult.Errors.PresentationTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySlugRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Slug: required, max length, valid format
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonResult.Errors.SlugRequired.Code)
            .WithMessage(TaxonResult.Errors.SlugRequired.Message)
            .MaximumLength(TaxonConstant.Constraints.SlugMaxLength)
            .WithErrorCode(TaxonResult.Errors.SlugTooLong.Code)
            .WithMessage(TaxonResult.Errors.SlugTooLong.Message)
            .Matches(TaxonConstant.Patterns.Slug)
            .WithErrorCode(TaxonResult.Errors.SlugInvalidFormat.Code)
            .WithMessage(TaxonResult.Errors.SlugInvalidFormat.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Description: optional, max length
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.DescriptionMaxLength)
            .WithErrorCode(TaxonResult.Errors.DescriptionTooLong.Code)
            .WithMessage(TaxonResult.Errors.DescriptionTooLong.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        // Position: must be greater than or equal to minimum
        return ruleBuilder
            .GreaterThanOrEqualTo(TaxonConstant.Constraints.MinPosition)
            .WithErrorCode(TaxonResult.Errors.InvalidPosition.Code)
            .WithMessage(TaxonResult.Errors.InvalidPosition.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMetaTitleRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Meta title: optional, max length
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.MetaTitleMaxLength)
            .WithErrorCode(TaxonResult.Errors.MetaTitleTooLong.Code)
            .WithMessage(TaxonResult.Errors.MetaTitleTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMetaDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Meta description: optional, max length
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.MetaDescriptionMaxLength)
            .WithErrorCode(TaxonResult.Errors.MetaDescriptionTooLong.Code)
            .WithMessage(TaxonResult.Errors.MetaDescriptionTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMetaKeywordsRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Meta keywords: optional, max length
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.MetaKeywordsMaxLength)
            .WithErrorCode(TaxonResult.Errors.MetaKeywordsTooLong.Code)
            .WithMessage(TaxonResult.Errors.MetaKeywordsTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyImageUrlRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Image URL: optional, max length, valid format
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.UrlMaxLength)
            .WithErrorCode(TaxonResult.Errors.ImageUrlTooLong.Code)
            .WithMessage(TaxonResult.Errors.ImageUrlTooLong.Message)
            .Matches(TaxonConstant.Patterns.Url)
            .WithErrorCode(TaxonResult.Errors.ImageUrlInvalidFormat.Code)
            .WithMessage(TaxonResult.Errors.ImageUrlInvalidFormat.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySquareImageUrlRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        // Square image URL: optional, max length, valid format
        return ruleBuilder
            .MaximumLength(TaxonConstant.Constraints.UrlMaxLength)
            .WithErrorCode(TaxonResult.Errors.SquareImageUrlTooLong.Code)
            .WithMessage(TaxonResult.Errors.SquareImageUrlTooLong.Message)
            .Matches(TaxonConstant.Patterns.Url)
            .WithErrorCode(TaxonResult.Errors.SquareImageUrlInvalidFormat.Code)
            .WithMessage(TaxonResult.Errors.SquareImageUrlInvalidFormat.Message);
    }

    public static IRuleBuilderOptions<T, TaxonMatchPolicy> ApplyRulesMatchPolicyRules<T>(this IRuleBuilder<T, TaxonMatchPolicy> ruleBuilder)
    {
        // Rules match policy: must be valid enum value
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(TaxonResult.Errors.InvalidRulesMatchPolicy.Code)
            .WithMessage(TaxonResult.Errors.InvalidRulesMatchPolicy.Message);
    }

    public static IRuleBuilderOptions<T, TaxonSortOrder> ApplySortOrderRules<T>(this IRuleBuilder<T, TaxonSortOrder> ruleBuilder)
    {
        // Sort order: must be valid enum value
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(TaxonResult.Errors.InvalidSortOrder.Code)
            .WithMessage(TaxonResult.Errors.InvalidSortOrder.Message);
    }
}
