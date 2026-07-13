namespace Module.Ordering.Features.Admin.Orders.RemoveLineItem;

public static partial class RemoveOrderLineItem
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
        }
    }
}