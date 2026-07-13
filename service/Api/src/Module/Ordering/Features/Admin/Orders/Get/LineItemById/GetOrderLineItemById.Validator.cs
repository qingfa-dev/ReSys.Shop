using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

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
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            // Validate: Line item ID must not be empty.
            RuleFor(x => x.LineItemId)
                .NotEmpty()
                .WithErrorCode(LineItemResult.Errors.IdRequired.Code)
                .WithMessage(LineItemResult.Errors.IdRequired.Message);
        }
    }
}