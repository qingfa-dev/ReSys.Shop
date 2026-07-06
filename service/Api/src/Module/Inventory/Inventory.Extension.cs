using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Persistence.Seeders;
using Module.Inventory.Services;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory;

public static class InventoryExtension
{
    public static WebApplicationBuilder AddInventoryModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStockChecker, StockChecker>();
        builder.Services.AddHostedService<ReservationExpiryService>();

        builder.AddSeeder<StockLocationSeeder>();
        builder.AddSeeder<InventoryStockItemSeeder>();
        builder.AddSeeder<InventoryStockMovementSeeder>();

        return builder;
    }
}
