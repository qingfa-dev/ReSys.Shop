namespace Module.Catalog.Domain.Products.Options;

public static class ProductOptionTypeValidation
{
    public static IRuleBuilderOptions<T, Guid> ApplyProductIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ProductOptionTypeResult.Errors.ProductIdRequired.Code)
            .WithMessage(ProductOptionTypeResult.Errors.ProductIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyOptionTypeIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ProductOptionTypeResult.Errors.OptionTypeIdRequired.Code)
            .WithMessage(ProductOptionTypeResult.Errors.OptionTypeIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyPositionRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(ProductOptionTypeConstant.Constraints.MinPosition)
            .WithErrorCode(ProductOptionTypeResult.Errors.InvalidPosition.Code)
            .WithMessage(ProductOptionTypeResult.Errors.InvalidPosition.Message);
    }
}