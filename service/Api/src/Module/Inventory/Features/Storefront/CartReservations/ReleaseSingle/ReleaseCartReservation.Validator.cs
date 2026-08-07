namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public static partial class ReleaseCartReservation
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ReservationId).NotEmpty();
            RuleFor(x => x.CartToken).NotEmpty();
        }
    }
}
