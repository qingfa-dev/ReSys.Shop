using Microsoft.Extensions.DependencyInjection;

namespace Module.Shipping;

// @CAT-10 Boundary: Domain -> Infrastructure — do not import EF Core or persistence concerns above this line
// @CAT-10 Boundary: Module -> Host — this is the composition root for the Shipping module DI registration
public static class ShippingExtension
{
    // Register: Configure Shipping module services in the application DI container
    //           Currently provides calculator strategy services
    public static IServiceCollection AddShippingModule(this IServiceCollection services)
    {
        return services;
    }
}
