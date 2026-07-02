using Module.Location.Persistence.Seeders;

// Boundary: Domain → Infrastructure
namespace Module.Location;

public static class LocationExtensions
{
    public static WebApplicationBuilder AddLocationModule(this WebApplicationBuilder services)
    {
        services.AddSeeder<CountrySeeder>();
        services.AddSeeder<StateSeeder>();
        return services;
    }
}