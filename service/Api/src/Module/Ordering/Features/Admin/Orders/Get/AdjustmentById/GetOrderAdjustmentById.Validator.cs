using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Get.AdjustmentById;

public static partial class GetOrderAdjustmentById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.AdjustmentId)
                .NotEmpty()
                .WithErrorCode("Adjustment.Id.Required")
                .WithMessage("Adjustment ID is required.");
        }
    }
}
