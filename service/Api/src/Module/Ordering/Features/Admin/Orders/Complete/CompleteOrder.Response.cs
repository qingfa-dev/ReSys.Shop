using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.Ordering.Features.Admin.Orders.Complete;

public static partial class CompleteOrder
{
    public sealed record Response : OrderDetailResponse;
}