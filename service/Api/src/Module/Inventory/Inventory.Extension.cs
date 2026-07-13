using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Persistence.Seeders;
using Module.Inventory.Services;
using Module.Inventory.Services.Abstractions;

using Shared.Application.Contracts.Inventory;

namespace Module.Inventory;

public static class InventoryExtension
{
    public static WebApplicationBuilder AddInventoryModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStockQuantityService, StockQuantityService>();
        builder.Services.AddScoped<IStockAvailabilityService, StockAvailabilityService>();
        builder.Services.AddScoped<IStockReservationService, StockReservationService>();
        builder.Services.AddScoped<ICartReservationService, CartReservationService>();
        builder.Services.AddScoped<IStockRestockService, StockRestockService>();
        builder.Services.AddScoped<IStockSummaryService, StockSummaryService>();
        builder.Services.AddScoped<IStockAvailabilityCalculator, StockAvailabilityCalculator>();
        builder.Services.AddHostedService<ReservationExpiryService>();

        builder.AddSeeder<StockLocationSeeder>();
        builder.AddSeeder<InventoryStockItemSeeder>();
        builder.AddSeeder<InventoryStockMovementSeeder>();

        return builder;
    }
}