using Module.Profile.Persistence.Seeders;

namespace Module.Profile;

public static class ProfilesExtensions
{
    public static WebApplicationBuilder AddProfilesModule(this WebApplicationBuilder services)
    {
        services.AddSeeder<UserProfileSeeder>();
        services.AddSeeder<AddressSeeder>();

        return services;
    }
}