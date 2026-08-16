using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Cancel;

public static partial class CancelOrderAdmin
{
    public sealed record Request : OrderCancellationParameters;
}