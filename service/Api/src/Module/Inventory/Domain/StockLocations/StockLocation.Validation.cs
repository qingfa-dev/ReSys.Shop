namespace Module.Inventory.Domain.StockLocations;

public static class StockLocationValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyAdminNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.AdminNameMaxLength)
            .WithErrorCode(StockLocationResult.Failure.AdminNameTooLong.Code)
            .WithMessage(StockLocationResult.Failure.AdminNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(StockLocationResult.Failure.PresentationTooLong.Code)
            .WithMessage(StockLocationResult.Failure.PresentationTooLong.Message);
    }
}
