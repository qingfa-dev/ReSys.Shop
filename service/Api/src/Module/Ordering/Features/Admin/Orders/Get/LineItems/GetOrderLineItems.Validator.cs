namespace Module.Ordering.Features.Admin.Orders.Get.LineItems;

public static partial class GetOrderLineItems
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.Parameters)
                .NotNull()
                .WithErrorCode("Order.Parameters.Required")
                .WithMessage("Query parameters are required.");
        }
    }
}
