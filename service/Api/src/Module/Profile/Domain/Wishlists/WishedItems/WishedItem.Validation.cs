// Validate: FluentValidation rules for WishedItem — quantity bounds and required GUIDs
namespace Module.Profile.Domain.Wishlists.WishedItems;

public static class WishedItemValidation
{
    public static IRuleBuilderOptions<T, Guid> ApplyVariantIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WishedItemResult.Failure.VariantIdRequired.Code)
            .WithMessage(WishedItemResult.Failure.VariantIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyWishlistIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WishedItemResult.Failure.WishlistIdRequired.Code)
            .WithMessage(WishedItemResult.Failure.WishlistIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, int> ApplyQuantityRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(WishedItemConstant.Constraints.MinQuantity)
            .WithErrorCode(WishedItemResult.Failure.QuantityTooLow.Code)
            .WithMessage(WishedItemResult.Failure.QuantityTooLow.Message)
            .LessThanOrEqualTo(WishedItemConstant.Constraints.MaxQuantity)
            .WithErrorCode(WishedItemResult.Failure.QuantityTooHigh.Code)
            .WithMessage(WishedItemResult.Failure.QuantityTooHigh.Message);
    }
}
