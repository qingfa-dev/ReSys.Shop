using Module.Shipping.Persistence.Seeders;

namespace Module.Shipping;

// @CAT-10 Boundary: Domain -> Infrastructure — do not import EF Core or persistence concerns above this line
// @CAT-10 Boundary: Module -> Host — this is the composition root for the Shipping module DI registration
public static class ShippingExtension
{
    /// <summary>
    /// Registers Shipping module services into the dependency injection container.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static WebApplicationBuilder AddShippingModule(this WebApplicationBuilder builder)
    {
        // Register: Seeders
        builder.AddSeeder<ShippingMethodSeeder>();
        builder.AddSeeder<ShippingRateSeeder>();

        return builder;
    }
}
