using Module.Profile.Domain.Wishlists.WishedItems;

namespace Module.Profile.Features.Store.Wishlists.AddItem;

public static partial class AddWishlistItem
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty();

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(WishedItemConstant.Constraints.MinQuantity)
                .LessThanOrEqualTo(WishedItemConstant.Constraints.MaxQuantity);
        }
    }
}