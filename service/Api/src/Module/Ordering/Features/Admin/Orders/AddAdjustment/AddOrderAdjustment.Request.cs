namespace Module.Ordering.Features.Admin.Orders.AddAdjustment;

public static partial class AddOrderAdjustment
{
    public class Request
    {
        public string Label { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public bool Inclusive { get; init; }
    }
}
