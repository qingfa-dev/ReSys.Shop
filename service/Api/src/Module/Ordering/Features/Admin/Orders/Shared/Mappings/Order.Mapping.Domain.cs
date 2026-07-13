using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Mappings;

/// <summary>Maps order request DTOs to domain entities for the admin order features.</summary>
// Boundary: Features → Domain — converts application-layer DTOs before persistence
public static partial class OrderMapping
{
    /// <summary>Maps an OrderRequest DTO to an Order domain entity with user and store identifiers.</summary>
    /// <typeparam name="T">The request type (must inherit from OrderRequest).</typeparam>
    /// <param name="request">The incoming order request DTO.</param>
    /// <param name="userId">The identifier of the user creating the order.</param>
    /// <param name="storeId">The identifier of the store the order belongs to.</param>
    /// <returns>A Result containing the new Order entity, or error details.</returns>
    // Map: Request DTO -> Domain entity
    public static Result<Order> MapToDomain<T>(this T request, Guid userId, Guid storeId) where T : OrderRequest
    {
        // Create: Build order domain entity with configured defaults (currency, timestamps).
        return OrderMethod.Create(
            currency: request.Currency,
            userId: userId,
            storeId: storeId);
    }
}
