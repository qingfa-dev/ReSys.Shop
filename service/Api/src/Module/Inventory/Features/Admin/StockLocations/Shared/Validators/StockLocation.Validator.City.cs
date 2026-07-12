using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyCityRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.CityMaxLength)
            .WithErrorCode(StockLocationResult.Failure.CityTooLong.Code)
            .WithMessage(StockLocationResult.Failure.CityTooLong.Message);
    }
}
