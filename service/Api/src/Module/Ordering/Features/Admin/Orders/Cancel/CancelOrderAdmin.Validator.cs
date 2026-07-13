namespace Module.Ordering.Features.Admin.Orders.Cancel;

public static partial class CancelOrderAdmin
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode("Order.Request.Required")
                .WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.Reason)
                    .MaximumLength(500)
                    .WithErrorCode("Order.Reason.TooLong")
                    .WithMessage("Cancellation reason must be 500 characters or fewer.");
            });
        }
    }
}
