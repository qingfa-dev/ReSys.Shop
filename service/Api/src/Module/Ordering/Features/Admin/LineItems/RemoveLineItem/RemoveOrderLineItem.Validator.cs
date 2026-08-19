using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.LineItems.RemoveLineItem;

public static partial class RemoveOrderLineItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.LineItemIdRequired.Code)
                .WithMessage(OrderResult.Errors.LineItemIdRequired.Message);
        }
    }
}