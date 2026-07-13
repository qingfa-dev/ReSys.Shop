using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(StockLocationResult.Failure.NameRequired.Code)
            .WithMessage(StockLocationResult.Failure.NameRequired.Message)
            .MaximumLength(StockLocationConstant.Constraints.NameMaxLength)
            .WithErrorCode(StockLocationResult.Failure.NameTooLong.Code)
            .WithMessage(StockLocationResult.Failure.NameTooLong.Message);
    }
}