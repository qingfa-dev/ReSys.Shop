namespace Module.Catalog.Domain.Products.Classifications;

public static class ClassificationValidation
{
    // Validate: Product ID must be non-empty for classification
    public static IRuleBuilderOptions<T, Guid> ApplyProductIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ClassificationResult.Errors.ProductIdRequired.Code)
            .WithMessage(ClassificationResult.Errors.ProductIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyTaxonIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ClassificationResult.Errors.TaxonIdRequired.Code)
            .WithMessage(ClassificationResult.Errors.TaxonIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(ClassificationConstant.Constraints.MinPosition)
            .WithErrorCode(ClassificationResult.Errors.InvalidPosition.Code)
            .WithMessage(ClassificationResult.Errors.InvalidPosition.Message);
    }
}