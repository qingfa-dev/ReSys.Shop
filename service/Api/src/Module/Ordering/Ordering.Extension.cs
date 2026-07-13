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
        // Register: Cart expiry background components
        // Note: Both CartExpiryService (BackgroundService) and CartExpiryJobScheduler
        // (Hangfire IHostedService) are registered. The BackgroundService runs on a
        // simple 1-hour interval as a fallback. The Hangfire scheduler is the
        // preferred mechanism in environments where Hangfire is configured. Both are
        // idempotent — double-expiring the same cart is safe (status check + skip).
        builder.Services.AddScoped<Backgrounds.CartExpiryJob>();
        builder.Services.AddHostedService<Services.CartExpiryService>();
        builder.Services.AddHostedService<Backgrounds.CartExpiryJobScheduler>();

        // Register: Seeders for development database initialization
        builder.AddSeeder<OrderSeeder>();
        builder.AddSeeder<PaymentSeeder>();

        return builder;
    }
}