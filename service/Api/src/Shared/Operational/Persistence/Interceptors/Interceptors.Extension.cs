using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Operational.Persistence.Interceptors;

public static class InterceptorsExtension
{

    /// <summary>
    /// Registers the standard set of EF Core interceptors for auditing, soft delete, and concurrency.
    /// </summary>
    /// <param name="services">The service collection to add interceptors to.</param>
    internal static void AddPersistenceInterceptors(this IServiceCollection services)
    {
        // Add: Auditing interceptor to automatically populate audit fields
        services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();

        // Add: Soft delete interceptor to manage logical deletion
        services.AddScoped<ISaveChangesInterceptor, SoftDeletableInterceptor>();

        // Add: Concurrency interceptor to manage optimistic concurrency via versioning
        services.AddScoped<ISaveChangesInterceptor, VersionableInterceptor>();
    }

}