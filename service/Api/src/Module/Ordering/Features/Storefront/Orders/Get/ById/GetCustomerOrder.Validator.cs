namespace Module.Ordering.Features.Storefront.Orders.Get.ById;

public static partial class GetCustomerOrder
{
    /// <summary>Validates the get-customer-order query: order ID must not be empty.</summary>
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            // Validate: Order ID must not be empty.
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}