using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockReservations.Shared.Validators;

public static partial class StockReservationValidator
{
    // Validate: Stock reservation parameters rules
    public sealed class StockReservationParametersValidator : AbstractValidator<StockReservationParameters>
    {
        public StockReservationParametersValidator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty()
                .WithErrorCode("StockReservation.Variant.Required")
                .WithMessage("Variant is required.");

            RuleFor(x => x.Quantity)
                .ApplyQuantityRules();

            RuleFor(x => x.Reason)
                .MaximumLength(StockReservationConstant.Constraints.MaxReasonLength)
                .WithErrorCode("StockReservation.Reason.TooLong")
                .WithMessage($"Reason cannot exceed {StockReservationConstant.Constraints.MaxReasonLength} characters.");
        }
    }

    public static IRuleBuilderOptions<T, StockReservationParameters> ApplyStockReservationParametersRules<T>(
        this IRuleBuilder<T, StockReservationParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockReservationParametersValidator());
    }
}
