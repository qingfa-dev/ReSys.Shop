namespace Module.Inventory.Features.Admin.StockReservations.Cancel;

public static partial class CancelStockReservation
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("StockReservation.Cancel.IdRequired")
                .WithMessage("Reservation identifier is required.");
        }
    }
}
