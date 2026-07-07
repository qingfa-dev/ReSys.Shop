namespace Module.Promotions.Domain.Promotions;

public static class PromotionValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PromotionResult.Errors.NameRequired.Code)
            .WithMessage(PromotionResult.Errors.NameRequired.Description)
            .MaximumLength(PromotionConstant.Constraints.MaxNameLength)
            .WithErrorCode(PromotionResult.Errors.NameTooLong.Code)
            .WithMessage(PromotionResult.Errors.NameTooLong.Description);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PromotionConstant.Constraints.MaxCodeLength)
            .WithErrorCode(PromotionResult.Errors.CodeTooLong.Code)
            .WithMessage(PromotionResult.Errors.CodeTooLong.Description);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PromotionConstant.Constraints.MaxDescriptionLength)
            .WithErrorCode(PromotionResult.Errors.DescriptionTooLong.Code)
            .WithMessage(PromotionResult.Errors.DescriptionTooLong.Description);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPathRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(PromotionConstant.Constraints.MaxPathLength)
            .WithErrorCode(PromotionResult.Errors.PathTooLong.Code)
            .WithMessage(PromotionResult.Errors.PathTooLong.Description);
    }

    public static IRuleBuilderOptions<T, MatchPolicy> ApplyMatchPolicyRules<T>(this IRuleBuilder<T, MatchPolicy> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(PromotionResult.Errors.InvalidMatchPolicy.Code)
            .WithMessage(PromotionResult.Errors.InvalidMatchPolicy.Description);
    }

    public static IRuleBuilderOptions<T, PromotionKind> ApplyKindRules<T>(this IRuleBuilder<T, PromotionKind> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(PromotionResult.Errors.InvalidKind.Code)
            .WithMessage(PromotionResult.Errors.InvalidKind.Description);
    }

    public static IRuleBuilderOptions<T, DateTimeOffset?> ApplyExpiryRangeRules<T>(this IRuleBuilder<T, DateTimeOffset?> ruleBuilder)
    {
        return ruleBuilder
            .Must((promotion, expiresAt, context) =>
            {
                if (context.InstanceToValidate is Promotion p && expiresAt.HasValue && p.StartsAtUtc.HasValue)
                    return expiresAt.Value > p.StartsAtUtc.Value;
                return true;
            })
            .WithErrorCode(PromotionResult.Errors.ExpiresAtBeforeStartsAt.Code)
            .WithMessage(PromotionResult.Errors.ExpiresAtBeforeStartsAt.Description);
    }
}