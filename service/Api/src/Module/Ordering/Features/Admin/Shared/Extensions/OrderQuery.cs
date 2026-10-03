using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Shared.Extensions;

/// <summary>Query extensions for loading Order aggregates with their full detail navigations.</summary>
// Boundary: Persistence -> Domain — centralizes the include set required to safely materialize Order detail.
public static class OrderQuery
{
    /// <summary>Includes the full set of navigations dereferenced by order-detail mapping (LineItems, Adjustments, PaymentCaptures, Shipments + ShippingMethod).</summary>
    // Include: Loads every collection the detail mapper touches so no navigation is null under the relational provider.
    public static IQueryable<Order> IncludeOrderDetail(this IQueryable<Order> query) =>
        query.Include(o => o.LineItems)
             .Include(o => o.Adjustments)
             .Include(o => o.PaymentCaptures)
             .Include(o => o.Shipments)
                 .ThenInclude(s => s.ShippingMethod);
}
