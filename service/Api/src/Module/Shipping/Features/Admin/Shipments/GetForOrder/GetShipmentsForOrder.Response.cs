using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.ListForOrder;

public static partial class GetShipmentsForOrder
{
    public sealed record Response : ShipmentListItemResponse;
}
