using Module.Inventory.Services.Models;

namespace Module.Inventory.Features.Admin.StockItems.Summary;

public static partial class GetStockSummary
{
    public class Response : VariantStockSummary
    {
        public Response() { }

        public Response(VariantStockSummary other)
        {
            VariantId = other.VariantId;
            TotalOnHand = other.TotalOnHand;
            TotalReserved = other.TotalReserved;
            TotalAvailable = other.TotalAvailable;
            LocationBreakdown = other.LocationBreakdown;
        }
    }
}
