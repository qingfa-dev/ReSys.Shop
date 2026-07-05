using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Services;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory;

// @CAT-10 Boundary: Domain -> Application — module registration boundary; do not add domain logic here
public static class InventoryExtension
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<IStockChecker, StockChecker>();
        services.AddHostedService<ReservationExpiryService>();
        return services;
    }
}
