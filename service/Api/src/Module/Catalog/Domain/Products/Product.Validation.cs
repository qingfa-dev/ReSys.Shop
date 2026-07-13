namespace Module.Catalog.Domain.Products;

public static class ProductValidation
{
    // Validate: Product name must be non-empty and within maximum length
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ProductResult.Errors.NameRequired.Code)
            .WithMessage(ProductResult.Errors.NameRequired.Message)
            .MaximumLength(ProductConstant.Constraints.MaxNameLength)
            .WithErrorCode(ProductResult.Errors.NameTooLong.Code)
            .WithMessage(ProductResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySlugRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ProductResult.Errors.SlugRequired.Code)
            .WithMessage(ProductResult.Errors.SlugRequired.Message)
            .MaximumLength(ProductConstant.Constraints.MaxSlugLength)
            .WithErrorCode(ProductResult.Errors.SlugTooLong.Code)
            .WithMessage(ProductResult.Errors.SlugTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ProductConstant.Constraints.MaxDescriptionLength)
            .WithErrorCode(ProductResult.Errors.DescriptionTooLong.Code)
            .WithMessage(ProductResult.Errors.DescriptionTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMetaTitleRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ProductConstant.Constraints.MaxMetaTitleLength)
            .WithErrorCode(ProductResult.Errors.MetaTitleTooLong.Code)
            .WithMessage(ProductResult.Errors.MetaTitleTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMetaDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ProductConstant.Constraints.MaxMetaDescriptionLength)
            .WithErrorCode(ProductResult.Errors.MetaDescriptionTooLong.Code)
            .WithMessage(ProductResult.Errors.MetaDescriptionTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMetaKeywordsRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ProductConstant.Constraints.MaxMetaKeywordsLength)
            .WithErrorCode(ProductResult.Errors.MetaKeywordsTooLong.Code)
            .WithMessage(ProductResult.Errors.MetaKeywordsTooLong.Message);
    }

    public static IRuleBuilderOptions<T, ProductStatus> ApplyStatusRules<T>(this IRuleBuilder<T, ProductStatus> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(ProductResult.Errors.InvalidStatus.Code)
            .WithMessage(ProductResult.Errors.InvalidStatus.Message);
    }

    public static IRuleBuilderOptions<T, DateTimeOffset?> ApplyStatusTransitionRules<T>(this IRuleBuilder<T, DateTimeOffset?> ruleBuilder)
    {
        return ruleBuilder
            .Must((product, discontinueOn, context) =>
            {
                if (context.InstanceToValidate is Product p && discontinueOn.HasValue && p.AvailableOn.HasValue)
                    return discontinueOn.Value > p.AvailableOn.Value;
                return true;
            })
            .WithErrorCode("Product.StatusTransition.Invalid")
            .WithMessage("Discontinue date must be later than the available-on date.");
    }

    public static void ApplyFashionFieldRules(this AbstractValidator<Product> validator)
    {
        validator.RuleFor(x => x.StyleCode)
            .MaximumLength(ProductConstant.Constraints.MaxStyleCodeLength);

        validator.RuleFor(x => x.SeasonName)
            .MaximumLength(ProductConstant.Constraints.MaxSeasonNameLength);

        validator.RuleFor(x => x.MaterialComposition)
            .MaximumLength(ProductConstant.Constraints.MaxMaterialCompositionLength);

        validator.RuleFor(x => x.CareInstructions)
            .MaximumLength(ProductConstant.Constraints.MaxCareInstructionsLength);

        validator.RuleFor(x => x.FitNotes)
            .MaximumLength(ProductConstant.Constraints.MaxFitNotesLength);

        validator.RuleFor(x => x.Department)
            .MaximumLength(ProductConstant.Constraints.MaxDepartmentLength);

        validator.RuleFor(x => x.GenderTarget)
            .MaximumLength(ProductConstant.Constraints.MaxGenderTargetLength);
    }
}