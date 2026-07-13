using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Features.Admin.Orders.UpdateLineItem;

public static partial class UpdateOrderLineItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode("Order.LineItemId.Required")
                .WithMessage("Line item ID is required.");

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode("Order.Request.Required")
                .WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.Quantity)
                    .InclusiveBetween(1, LineItemConstant.MaxQuantity)
                    .WithErrorCode("Order.Quantity.Invalid")
                    .WithMessage($"Quantity must be between 1 and {LineItemConstant.MaxQuantity}.");
            });
        }
    }
}