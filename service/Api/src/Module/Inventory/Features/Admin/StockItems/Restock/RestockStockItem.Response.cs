using Module.Inventory.Services;

namespace Module.Inventory.Features.Admin.StockItems.Restock;

public static partial class RestockStockItem
{
    public class Response : RestockResult
    {
        public Response() { }

        public Response(RestockResult other)
        {
            StockItemId = other.StockItemId;
            PreviousCountOnHand = other.PreviousCountOnHand;
            NewCountOnHand = other.NewCountOnHand;
            BackordersFulfilled = other.BackordersFulfilled;
            PartiallyFulfilled = other.PartiallyFulfilled;
            RemainingQuantity = other.RemainingQuantity;
            MovementId = other.MovementId;
        }
    }
}
