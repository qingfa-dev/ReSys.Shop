using FluentValidation;

using Module.Inventory.Features.Storefront.Shared.Validators;

namespace Module.Inventory.Features.Storefront.StockReservations.ConsumeCart;

public static partial class ConsumeCartStockReservations
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.CartId).MustBeValidCartId();
        }
    }
}
