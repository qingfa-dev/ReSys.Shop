using Module.Customer.Persistence.Seeders;

namespace Module.Customer;

public static class ProfilesExtensions
{
    public static WebApplicationBuilder AddCustomerModule(this WebApplicationBuilder services)
    {
        services.AddSeeder<UserProfileSeeder>();
        services.AddSeeder<AddressSeeder>();

        return services;
    }
}