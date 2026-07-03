namespace Module.Catalog.Domain.Products.Variants.Images;

public static class VariantImageValidation
{
    // Validate: Content type must be non-empty and within maximum length
    public static IRuleBuilderOptions<T, string?> ApplyContentTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(VariantImageConstant.Constraints.ContentTypeMaxLength)
            .WithErrorCode(VariantImageResult.Failure.ContentTypeTooLong.Code)
            .WithMessage(VariantImageResult.Failure.ContentTypeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyFileNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(VariantImageConstant.Constraints.FileNameMaxLength)
            .WithErrorCode(VariantImageResult.Failure.FileNameTooLong.Code)
            .WithMessage(VariantImageResult.Failure.FileNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyFileSizeRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(VariantImageResult.Failure.InvalidFileSize.Code)
            .WithMessage(VariantImageResult.Failure.InvalidFileSize.Message);
    }

    public static IRuleBuilderOptions<T, int?> ApplyWidthRules<T>(this IRuleBuilder<T, int?> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(VariantImageConstant.Constraints.MinDimension, VariantImageConstant.Constraints.MaxDimension)
            .WithErrorCode(VariantImageResult.Failure.InvalidDimension.Code)
            .WithMessage(VariantImageResult.Failure.InvalidDimension.Message);
    }

    public static IRuleBuilderOptions<T, int?> ApplyHeightRules<T>(this IRuleBuilder<T, int?> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(VariantImageConstant.Constraints.MinDimension, VariantImageConstant.Constraints.MaxDimension)
            .WithErrorCode(VariantImageResult.Failure.InvalidDimension.Code)
            .WithMessage(VariantImageResult.Failure.InvalidDimension.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDimensionsUnitRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(unit => string.IsNullOrEmpty(unit) || VariantImageConstant.Constraints.Dimensions.AllowedUnits.Contains(unit))
            .WithErrorCode(VariantImageResult.Failure.InvalidDimensionsUnit.Code)
            .WithMessage(VariantImageResult.Failure.InvalidDimensionsUnit.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(VariantImageConstant.Constraints.MinPosition)
            .WithErrorCode(VariantImageResult.Failure.InvalidPosition.Code)
            .WithMessage(VariantImageResult.Failure.InvalidPosition.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyUrlRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(VariantImageResult.Failure.UrlRequired.Code)
            .WithMessage(VariantImageResult.Failure.UrlRequired.Message)
            .MaximumLength(VariantImageConstant.Constraints.UrlMaxLength)
            .WithErrorCode(VariantImageResult.Failure.UrlTooLong.Code)
            .WithMessage(VariantImageResult.Failure.UrlTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAltRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(VariantImageConstant.Constraints.AltMaxLength)
            .WithErrorCode(VariantImageResult.Failure.AltTooLong.Code)
            .WithMessage(VariantImageResult.Failure.AltTooLong.Message);
    }

    public static IRuleBuilderOptions<T, VariantImageType> ApplyTypeRules<T>(this IRuleBuilder<T, VariantImageType> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(VariantImageResult.Failure.InvalidType.Code)
            .WithMessage(VariantImageResult.Failure.InvalidType.Message);
    }
}
