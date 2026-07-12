namespace Module.Ordering.Features.Admin.Orders.Approve;

public static partial class ApproveOrder
{
    /// <summary>Validates the ApproveOrder command — order ID must be provided.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");
        }
    }
}
