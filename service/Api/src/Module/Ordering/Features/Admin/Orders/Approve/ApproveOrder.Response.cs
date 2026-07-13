namespace Module.Ordering.Features.Admin.Orders.Approve;
public static partial class ApproveOrder
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        /// <summary>Identity of the administrator who approved the order — null if approval was system-initiated.</summary>
        public Guid? ApprovedById { get; init; }
        /// <summary>UTC timestamp of when the approval was recorded.</summary>
        public DateTimeOffset? ApprovedAtUtc { get; init; }
    }
}
