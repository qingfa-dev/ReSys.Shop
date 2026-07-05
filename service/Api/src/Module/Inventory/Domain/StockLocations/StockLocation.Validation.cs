namespace Module.Inventory.Domain.StockLocations;

public static class StockLocationValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyAdminNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.AdminNameMaxLength)
            .WithErrorCode(StockLocationResult.Errors.AdminNameTooLong.Code)
            .WithMessage(StockLocationResult.Errors.AdminNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PresentationTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PresentationTooLong.Message);
    }
}
