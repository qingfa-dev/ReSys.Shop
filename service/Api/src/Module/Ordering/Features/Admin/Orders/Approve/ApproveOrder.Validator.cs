using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Approve;

public static partial class ApproveOrder
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");
        }
    }
}
