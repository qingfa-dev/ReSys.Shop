using Module.Location.Persistence.Seeders;

// Boundary: Domain → Infrastructure
namespace Module.Location;

public static class LocationsExtensions
{
    public static WebApplicationBuilder AddLocationsModule(this WebApplicationBuilder services)
    {
        services.AddSeeder<CountrySeeder>();
        services.AddSeeder<StateSeeder>();
        return services;
    }
}