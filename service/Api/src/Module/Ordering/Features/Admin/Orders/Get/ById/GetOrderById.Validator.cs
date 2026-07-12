namespace Module.Ordering.Features.Admin.Orders.Get.ById;

public static partial class GetOrderById
{
    /// <summary>Validates the GetOrderById query — order ID must be provided.</summary>
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
