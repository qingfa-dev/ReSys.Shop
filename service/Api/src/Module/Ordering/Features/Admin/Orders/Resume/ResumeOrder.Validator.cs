using FluentValidation;

namespace Module.Ordering.Features.Admin.Orders.Resume;

public static partial class ResumeOrder
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
