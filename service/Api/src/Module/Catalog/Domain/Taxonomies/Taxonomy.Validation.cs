namespace Module.Catalog.Domain.Taxonomies;

public static class TaxonomyValidation
{
    // Validate: Taxonomy name must be non-empty and within maximum length
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonomyResult.Errors.NameRequired.Code)
            .WithMessage(TaxonomyResult.Errors.NameRequired.Message)
            .MaximumLength(TaxonomyConstant.Constraints.NameMaxLength)
            .WithErrorCode(TaxonomyResult.Errors.NameTooLong.Code)
            .WithMessage(TaxonomyResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonomyResult.Errors.PresentationRequired.Code)
            .WithMessage(TaxonomyResult.Errors.PresentationRequired.Message)
            .MaximumLength(TaxonomyConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(TaxonomyResult.Errors.PresentationTooLong.Code)
            .WithMessage(TaxonomyResult.Errors.PresentationTooLong.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(TaxonomyConstant.Constraints.MinPosition)
            .WithErrorCode(TaxonomyResult.Errors.InvalidPosition.Code)
            .WithMessage(TaxonomyResult.Errors.InvalidPosition.Message);
    }
}
