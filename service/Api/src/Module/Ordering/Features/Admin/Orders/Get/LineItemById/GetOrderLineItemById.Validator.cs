namespace Module.Ordering.Features.Admin.Orders.Get.LineItemById;

public static partial class GetOrderLineItemById
{
    /// <summary>Validates the GetOrderLineItemById query — both order ID and line item ID must be provided.</summary>
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            // Validate: Line item ID must not be empty.
            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode("LineItem.Id.Required")
                .WithMessage("Line item ID is required.");
        }
    }
}
