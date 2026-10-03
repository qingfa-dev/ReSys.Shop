using Module.Customer.Domain.Wishlists;

namespace Module.Customer.Features.Storefront.Wishlists.Update;

public static partial class UpdateWishlist
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            When(x => x.Name is not null, () =>
            {
                RuleFor(x => x.Name!)
                    .NotEmpty()
                    .MaximumLength(WishlistConstant.Constraints.MaxNameLength);
            });
        }
    }
}