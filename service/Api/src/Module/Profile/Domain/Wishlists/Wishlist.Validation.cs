// Validate: FluentValidation rules for Wishlist — name, token, and user identifier constraints
namespace Module.Profile.Domain.Wishlists;

public static class WishlistValidation
{
    public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WishlistResult.Failure.NameRequired.Code)
            .WithMessage(WishlistResult.Failure.NameRequired.Message)
            .MaximumLength(WishlistConstant.Constraints.MaxNameLength)
            .WithErrorCode(WishlistResult.Failure.NameTooLong.Code)
            .WithMessage(WishlistResult.Failure.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string> ApplyTokenRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WishlistResult.Failure.TokenRequired.Code)
            .WithMessage(WishlistResult.Failure.TokenRequired.Message)
            .MaximumLength(WishlistConstant.Constraints.MaxTokenLength)
            .WithErrorCode(WishlistResult.Failure.TokenTooLong.Code)
            .WithMessage(WishlistResult.Failure.TokenTooLong.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyUserIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WishlistResult.Failure.UserIdRequired.Code)
            .WithMessage(WishlistResult.Failure.UserIdRequired.Message);
    }
}