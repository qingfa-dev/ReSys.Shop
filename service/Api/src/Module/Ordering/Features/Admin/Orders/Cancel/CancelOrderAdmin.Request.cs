namespace Module.Ordering.Features.Admin.Orders.Cancel;

public static partial class CancelOrderAdmin
{
    public class Request
    {
        public string? Reason { get; init; }
    }
}
