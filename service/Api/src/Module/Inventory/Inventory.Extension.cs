using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Persistence.Seeders;
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory;

public static class InventoryExtension
{
    public static WebApplicationBuilder AddInventoryModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStockItemService, StockItemService>();
        builder.Services.AddScoped<IStockReservationService, StockReservationService>();
        builder.AddSeeder<StockLocationSeeder>();
        builder.AddSeeder<InventoryStockItemSeeder>();
        builder.AddSeeder<InventoryStockMovementSeeder>();

        builder.Services.AddScoped<Module.Inventory.Persistence.Seeders.DemoJsonHelper>();

        return builder;
    }
}