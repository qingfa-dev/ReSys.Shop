namespace Module.Ordering.Features.Storefront.Orders.Cancel;

public static partial class CancelOrder
{
    /// <summary>Validates the cancel-order command: order ID must not be empty.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
