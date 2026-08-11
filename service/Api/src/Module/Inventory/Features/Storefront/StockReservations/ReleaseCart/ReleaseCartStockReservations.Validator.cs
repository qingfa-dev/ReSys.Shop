using FluentValidation;

using Module.Inventory.Features.Storefront.Shared.Validators;

namespace Module.Inventory.Features.Storefront.StockReservations.ReleaseCart;

public static partial class ReleaseCartStockReservations
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.CartId).MustBeValidCartId();
        }
    }
}
