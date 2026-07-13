namespace Module.Ordering.Features.Admin.Orders.Resume;

public static partial class ResumeOrder
{
    /// <summary>Validates the ResumeOrder command — order ID must be provided.</summary>
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