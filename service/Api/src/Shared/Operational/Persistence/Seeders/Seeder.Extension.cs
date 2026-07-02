using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Operational.Persistence.Seeders;

/// <summary>
/// Provides extension methods for registering database data seeders.
/// </summary>
public static class Extensions
{
    #region Service Registration

    /// <summary>
    /// Registers a data seeder implementation with the dependency injection container.
    /// </summary>
    /// <typeparam name="TSeeder">The concrete type of the seeder to register.</typeparam>
    /// <param name="services">The service collection to add the seeder to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddSeeder<TSeeder>(this WebApplicationBuilder builder)
        where TSeeder : class, IDataSeeder
    {
        // Contract: pre=builder!=null, post=builder.Services.Contains(IDataSeeder)

        // Add: Register seeder as scoped service for consumption by DatabaseInitializer
        builder.Services.AddScoped<IDataSeeder, TSeeder>();
        
        return builder;
    }

    #endregion
}
