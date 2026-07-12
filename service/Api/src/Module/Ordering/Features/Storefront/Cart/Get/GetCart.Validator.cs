namespace Module.Ordering.Features.Storefront.Cart.Get;

public static partial class GetCart
{
    /// <summary>Validates the get-cart query: query object must not be null.</summary>
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            // Validate: Query must not be null.
            RuleFor(x => x).NotNull();
        }
    }
}
