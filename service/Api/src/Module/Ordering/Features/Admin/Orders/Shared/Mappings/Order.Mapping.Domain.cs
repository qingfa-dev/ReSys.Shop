using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Mappings;

public static partial class OrderMapping
{
    // Map: Request -> Domain entity (create)
    public static Result<Order> MapToDomain<T>(this T request, Guid userId, Guid storeId) where T : OrderRequest
    {
        return OrderExtensions.Create(
            currency: request.Currency,
            userId: userId,
            storeId: storeId);
    }
}
