using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

public static partial class AssociateCartWithUser
{
    public sealed record Request : CartAssociationParameters;
}