namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.VariantId).NotEmpty();
            RuleFor(x => x.StockLocationId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.TtlMinutes).InclusiveBetween(1, 10080);
        }
    }
}
