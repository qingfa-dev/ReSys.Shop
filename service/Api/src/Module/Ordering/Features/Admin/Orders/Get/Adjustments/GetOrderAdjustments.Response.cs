namespace Module.Ordering.Features.Admin.Orders.Get.Adjustments;
public static partial class GetOrderAdjustments
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Label { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public bool Eligible { get; init; }
        public bool Included { get; init; }
        public bool Mandatory { get; init; }
        public string State { get; init; } = string.Empty;
        public Guid SourceId { get; init; }
        public string SourceType { get; init; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
