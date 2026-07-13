using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.Create;

public static partial class CreateWishlist
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(WishlistConstant.Constraints.MaxNameLength);
        }
    }
}