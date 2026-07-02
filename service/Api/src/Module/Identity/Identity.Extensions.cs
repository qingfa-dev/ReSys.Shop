using Shared.Security.Identity.Seeders;

namespace Module.Identity;

public static class IdentityExtensions
{
    public static WebApplicationBuilder AddIdentityModule(this WebApplicationBuilder services)
    {
        services.AddSeeder<RoleSeeder>();
        services.AddSeeder<UserSeeder>();

        return services;
    }
}