namespace Module.Ordering.Features.Storefront.Cart.Get;

public static partial class GetCart
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x).NotNull();
        }
    }
}
