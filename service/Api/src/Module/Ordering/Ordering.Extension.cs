using Microsoft.Extensions.DependencyInjection;

using Module.Ordering.Domain.Orders;

namespace Module.Ordering;

// @CAT-10 Boundary: Ordering Module → DI Container — Module registration boundary; do not add domain logic here
public static class OrderingExtension
{
    /// <summary>
    /// Registers Ordering module services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    // @CAT-10 Boundary: Module DI registration entry point
    public static IServiceCollection AddOrderingModule(this IServiceCollection services)
    {
        services.AddScoped<IOrderEventPublisher, Infrastructure.Events.NullOrderEventPublisher>();
        services.AddScoped<Backgrounds.CartExpiryJob>();
        services.AddHostedService<Services.CartExpiryService>();
        return services;
    }
}
