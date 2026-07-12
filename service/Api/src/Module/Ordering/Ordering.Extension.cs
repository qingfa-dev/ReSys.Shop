using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Module.Ordering.Persistence.Seeders;

namespace Module.Ordering;

// @CAT-10 Boundary: Ordering Module → DI Container — Module registration boundary; do not add domain logic here
public static class OrderingExtension
{
    /// <summary>
    /// Registers Ordering module services into the dependency injection container.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    // @CAT-10 Boundary: Module DI registration entry point
    public static WebApplicationBuilder AddOrderingModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<Backgrounds.CartExpiryJob>();
        builder.Services.AddHostedService<Services.CartExpiryService>();

        // Register: Seeders
        builder.AddSeeder<OrderSeeder>();
        builder.AddSeeder<PaymentSeeder>();

        return builder;
    }
}
