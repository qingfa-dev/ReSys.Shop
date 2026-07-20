using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.StockLocationId).ApplyStockLocationRequired();
            RuleFor(x => x.Request.CartToken).NotEmpty()
                .WithErrorCode(StockReservationResult.Errors.CartTokenRequired.Code)
                .WithMessage(StockReservationResult.Errors.CartTokenRequired.Message);
            RuleFor(x => x.Request.VariantId).NotEmpty();
            RuleFor(x => x.Request.Quantity).ApplyQuantityRules();
            RuleFor(x => x.Request.TtlMinutes).ApplyTtlRangeRules();
        }
    }
}
