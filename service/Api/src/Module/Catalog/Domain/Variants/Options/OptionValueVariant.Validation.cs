namespace Module.Catalog.Domain.Variants.Options;

public static class OptionValueVariantValidation
{
    public static IRuleBuilderOptions<T, Guid> ApplyVariantIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionValueVariantResult.Errors.VariantIdRequired.Code)
            .WithMessage(OptionValueVariantResult.Errors.VariantIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyOptionValueIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OptionValueVariantResult.Errors.OptionValueIdRequired.Code)
            .WithMessage(OptionValueVariantResult.Errors.OptionValueIdRequired.Message);
    }
}