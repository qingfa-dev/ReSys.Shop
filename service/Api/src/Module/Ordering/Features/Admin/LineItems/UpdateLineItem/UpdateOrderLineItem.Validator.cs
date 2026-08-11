using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.LineItems.UpdateLineItem;

public static partial class UpdateOrderLineItem
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

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode(OrderResult.Errors.RequestRequired.Code)
                .WithMessage(OrderResult.Errors.RequestRequired.Message);

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.Quantity)
                    .ApplyQuantityRules();
            });
        }
    }
}