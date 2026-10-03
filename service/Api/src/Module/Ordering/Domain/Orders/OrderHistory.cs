using Shared.Application.Domain.Models;

namespace Module.Ordering.Domain.Orders;

/// <summary>
/// Immutable audit log entry recording state changes to an order.
/// </summary>
// @CAT-10 Invariant: Append-only; CreatedAtUtc is set at creation time; OrderId must reference an existing Order
// @CAT-10 Boundary: Domain → Persistence — EF Core entity; do not add persistence concerns to domain logic
public sealed class OrderHistory : Entity
{
    public Guid OrderId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
}