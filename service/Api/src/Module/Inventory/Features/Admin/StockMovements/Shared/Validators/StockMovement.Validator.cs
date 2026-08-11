using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Shared.Models;

namespace Module.Inventory.Features.Admin.StockMovements.Shared.Validators;

public static partial class StockMovementValidator
{
    // Validate: StockMovement parameters rules
    public sealed class StockMovementParametersValidator : AbstractValidator<StockMovementParameters>
    {
        public StockMovementParametersValidator()
        {
            RuleFor(x => x.StockItemId)
                .NotEmpty()
                .WithErrorCode("StockMovement.StockItem.Required")
                .WithMessage("Stock item is required.");

            RuleFor(x => x.Quantity)
                .ApplyQuantityRules();

            RuleFor(x => x.OriginatorType)
                .ApplyOriginatorTypeRules();

            RuleFor(x => x.Reason)
                .ApplyReasonRules();
        }
    }

    public static IRuleBuilderOptions<T, StockMovementParameters> ApplyStockMovementParametersRules<T>(
        this IRuleBuilder<T, StockMovementParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockMovementParametersValidator());
    }
}