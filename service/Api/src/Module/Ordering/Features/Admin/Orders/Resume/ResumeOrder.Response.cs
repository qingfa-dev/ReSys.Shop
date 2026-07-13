using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Resume;
public static partial class ResumeOrder
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        /// <summary>The order status after resume — restored from Canceled to its previous active state.</summary>
        public OrderStatus Status { get; init; }
    }
}
