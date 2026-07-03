namespace Module.Catalog.Domain.Products.Variants.Images.Embeddings;

public static class ImageEmbeddingValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyModelNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ImageEmbeddingResult.Errors.ModelNameRequired.Code)
            .WithMessage(ImageEmbeddingResult.Errors.ModelNameRequired.Message)
            .MaximumLength(ImageEmbeddingConstant.Constraints.ModelNameMaxLength)
            .WithErrorCode(ImageEmbeddingResult.Errors.ModelNameTooLong.Code)
            .WithMessage(ImageEmbeddingResult.Errors.ModelNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyModelVersionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ImageEmbeddingConstant.Constraints.ModelVersionMaxLength)
            .WithErrorCode(ImageEmbeddingResult.Errors.ModelVersionTooLong.Code)
            .WithMessage(ImageEmbeddingResult.Errors.ModelVersionTooLong.Message);
    }
}
