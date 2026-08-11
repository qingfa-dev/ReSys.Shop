using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Shared.Mappings;

/// <summary>Maps order request DTOs to domain entities for the admin order features.</summary>
// Boundary: Features → Domain — converts application-layer DTOs before persistence
public static partial class OrderMapping
{
    /// <summary>Maps an OrderRequest DTO to an Order domain entity with a user identifier.</summary>
    /// <typeparam name="T">The request type (must inherit from OrderRequest).</typeparam>
    /// <param name="request">The incoming order request DTO.</param>
    /// <param name="userId">The identifier of the user creating the order.</param>
    /// <returns>A Result containing the new Order entity, or error details.</returns>
    // Map: Request DTO -> Domain entity
    public static Result<Order> MapToDomain<T>(this T request, Guid userId) where T : OrderRequest
    {
        // Create: Build order domain entity with configured defaults (currency, timestamps).
        return OrderMethod.Create(
            currency: request.Currency,
            userId: userId);
    }

    /// <summary>Maps an OrderRequest DTO to an existing Order domain entity using patch semantics.</summary>
    /// <typeparam name="T">The request type (must inherit from OrderRequest).</typeparam>
    /// <param name="request">The incoming order request DTO with fields to update.</param>
    /// <param name="order">The existing Order entity to update.</param>
    /// <returns>A Result indicating success or validation failure.</returns>
    // Map: Request DTO -> existing Domain entity
    public static Result MapToDomain<T>(this T request, Order order) where T : OrderRequest
    {
        return order.UpdateDetails(
            email: request.Email,
            specialInstructions: request.SpecialInstructions,
            billAddressId: request.BillAddressId,
            shipAddressId: request.ShipAddressId,
            shippingMethodId: request.ShippingMethodId);
    }
}