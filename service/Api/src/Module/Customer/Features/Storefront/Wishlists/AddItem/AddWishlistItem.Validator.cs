using Module.Customer.Domain.Wishlists.WishedItems;

namespace Module.Customer.Features.Storefront.Wishlists.AddItem;

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