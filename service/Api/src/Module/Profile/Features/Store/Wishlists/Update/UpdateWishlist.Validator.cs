using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.Update;

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