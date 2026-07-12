using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.CodeMaxLength)
            .WithErrorCode(StockLocationResult.Failure.CodeTooLong.Code)
            .WithMessage(StockLocationResult.Failure.CodeTooLong.Message);
    }
}
