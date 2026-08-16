namespace Module.Shipping.Features.Admin.Shipments.ListForOrder;

public static partial class GetShipmentsForOrder
{
    public record Parameters : QueryingParameters
    {
        public Guid? OrderId { get; set; }
    }
}
