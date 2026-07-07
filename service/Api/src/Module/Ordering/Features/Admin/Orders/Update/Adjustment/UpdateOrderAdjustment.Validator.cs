using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Update.Adjustment;

public static partial class UpdateOrderAdjustment
{
    public sealed class Validator : AbstractValidator<Command>
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

            RuleFor(x => x.Request.Action)
                .IsInEnum()
                .WithErrorCode("Adjustment.Action.Required")
                .WithMessage("A valid adjustment action is required.");
        }
    }
}
