namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

public static partial class UpdateOrderStatus
{
    /// <summary>Validates the UpdateOrderStatus command — ID, request body, and status enum are required.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.Id).NotEmpty();
            // Validate: Request body must be provided.
            RuleFor(x => x.Request).NotNull();
            // Validate: Target status must be a valid OrderStatus enum value.
            RuleFor(x => x.Request.Status).IsInEnum();
        }
    }
}
