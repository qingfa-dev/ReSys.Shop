namespace Module.Ordering.Features.Admin.Orders.Get.LineItemById;

public static partial class GetOrderLineItemById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode("LineItem.Id.Required")
                .WithMessage("Line item ID is required.");
        }
    }
}
