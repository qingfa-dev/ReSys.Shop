using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(StockLocationResult.Errors.NameRequired.Code)
            .WithMessage(StockLocationResult.Errors.NameRequired.Message)
            .MaximumLength(StockLocationConstant.Constraints.NameMaxLength)
            .WithErrorCode(StockLocationResult.Errors.NameTooLong.Code)
            .WithMessage(StockLocationResult.Errors.NameTooLong.Message);
    }
}
