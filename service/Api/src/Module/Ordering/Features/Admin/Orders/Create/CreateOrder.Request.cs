using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Create;

public static partial class CreateOrder
{
    public sealed record Request : OrderRequest;
}
