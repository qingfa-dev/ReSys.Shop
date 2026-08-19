namespace Module.Catalog.Domain.Taxons.Rules;

public static class TaxonRuleValidation
{
    // Validate: Taxonomy ID must be non-empty for taxon rule association
    public static IRuleBuilderOptions<T, Guid> ApplyTaxonomyIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonRuleResult.Errors.TaxonomyIdRequired.Code)
            .WithMessage(TaxonRuleResult.Errors.TaxonomyIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyTaxonIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonRuleResult.Errors.TaxonIdRequired.Code)
            .WithMessage(TaxonRuleResult.Errors.TaxonIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyRuleIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonRuleResult.Errors.RuleIdRequired.Code)
            .WithMessage(TaxonRuleResult.Errors.RuleIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(TaxonRuleConstant.Constraints.TypeMaxLength)
            .Must(type => string.IsNullOrEmpty(type) || EnumExtensions.GetValues<TaxonRuleType>().Select(v => v.ToString()).Contains(type))
            .WithErrorCode(TaxonRuleResult.Errors.InvalidType.Code)
            .WithMessage(TaxonRuleResult.Errors.InvalidType.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyMatchPolicyRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(TaxonRuleConstant.Constraints.PolicyMaxLength)
            .Must(policy => string.IsNullOrEmpty(policy) || EnumExtensions.GetValues<TaxonRuleMatchPolicy>().Select(v => v.ToString()).Contains(policy))
            .WithErrorCode(TaxonRuleResult.Errors.InvalidMatchPolicy.Code)
            .WithMessage(TaxonRuleResult.Errors.InvalidMatchPolicy.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyValueRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(TaxonRuleResult.Errors.ValueRequired.Code)
            .WithMessage(TaxonRuleResult.Errors.ValueRequired.Message)
            .MaximumLength(TaxonRuleConstant.Constraints.ValueMaxLength)
            .WithErrorCode(TaxonRuleResult.Errors.ValueTooLong.Code)
            .WithMessage(TaxonRuleResult.Errors.ValueTooLong.Message);
    }
}