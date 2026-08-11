using FluentValidation;

using Module.Inventory.Features.Storefront.Shared.Validators;

namespace Module.Inventory.Features.Storefront.StockReservations.ReserveCart;

public static partial class ReserveCartStock
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.CartId).MustBeValidCartId();
            RuleFor(x => x.Request.LineItems).MustHaveValidLineItems();
            RuleFor(x => x.Request.TtlMinutes).MustBeValidTtlMinutes();
        }
    }
}
