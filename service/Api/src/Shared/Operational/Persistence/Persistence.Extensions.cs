using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Interceptors;
using Shared.Operational.Persistence.Options;

namespace Shared.Operational.Persistence;

/// <summary>
/// Provides extension methods for registering and configuring persistence services and database contexts.
/// </summary>
public static class PersistenceExtensions
{
    #region Internal Helpers

    #endregion

    #region Service Registration

    /// <summary>
    /// Registers the primary application database context with Npgsql and vector support.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for entity configurations.</param>
    /// <returns>The web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddPersistence(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        // Contract: pre=builder!=null, post=builder.Services.Contains(ApplicationDbContext)

        #region Interceptors
        // Initialize: Register core persistence interceptors in DI container
        builder.Services.AddPersistenceInterceptors();
        #endregion


        #region DbContext Configuration
        // Assign: Scan additional assemblies for entity configurations during startup
        if (additionalAssemblies.Length > 0)
        {
            ApplicationDbContext.AdditionalConfigurationsAssemblies = additionalAssemblies;
        }

        // Add: Configure primary application database context with Npgsql and interceptors
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            // Compute: Resolve connection string key based on Aspire environment state
            var resolvedName = builder.Configuration.GetConnectionString(DatabaseOption.Aspire) != null
                ? DatabaseOption.Aspire
                : DatabaseOption.Default;

            string? connectionString = builder.Configuration.GetConnectionString(resolvedName);

            // Validate: Ensure a valid connection string is provided for the database
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{resolvedName}' is missing. Please provide it in your configuration.");
            }

            // Assign: Link all registered interceptors from the service provider
            IEnumerable<ISaveChangesInterceptor> interceptors = sp.GetServices<ISaveChangesInterceptor>();
            options.AddInterceptors(interceptors);

            // Initialize: Configure Npgsql provider with vector support and migration assembly
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
                // Assign: Set migrations assembly to Api.Migrations project
                npgsqlOptions.MigrationsAssembly("Api.Migrations");
            });

            // Initialize: Apply snake_case naming convention for PostgreSQL compatibility
            options.UseSnakeCaseNamingConvention();

            // Log: Enable sensitive data and detailed error reporting in development environments only
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // Suppress: Ignore non-critical EF Core warnings for startup compatibility
            options.ConfigureWarnings(warnings =>
            {
                warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
            });
        });

        #endregion

        #region Interfaces & Initialization

        // Map: Map IApplicationDbContext interface to concrete ApplicationDbContext implementation
        builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        #endregion

        return builder;
    }

    /// <summary>
    /// Registers a generic database context with Npgsql support and standard persistence interceptors.
    /// </summary>
    /// <typeparam name="TInterface">The interface type for the context.</typeparam>
    /// <typeparam name="TContext">The concrete DbContext type.</typeparam>
    /// <param name="builder">The web application builder.</param>
    /// <param name="connectionName">Optional custom connection string name.</param>
    /// <returns>The web application builder for method chaining.</returns>
    public static WebApplicationBuilder AddNpgsqlDbContext<TInterface, TContext>(
        this WebApplicationBuilder builder,
        string? connectionName = null)
        where TContext : DbContext, TInterface
        where TInterface : class
    {
        // Contract: pre=builder!=null, post=builder.Services.Contains(TContext)

        #region Interceptors

        // Initialize: Register core persistence interceptors in DI container
        builder.Services.AddPersistenceInterceptors();

        #endregion

        #region DbContext Registration

        // Add: Configure generic DbContext with Npgsql and interceptors
        builder.Services.AddDbContext<TContext>((sp, options) =>
        {
            // Compute: Resolve connection string key using provided or default logic
            var resolvedName = connectionName
                               ?? (builder.Configuration.GetConnectionString(DatabaseOption.Aspire) != null
                                   ? DatabaseOption.Aspire
                                   : DatabaseOption.Default);

            var connectionString = builder.Configuration.GetConnectionString(resolvedName);

            // Validate: Ensure connection string is present for the generic context
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{resolvedName}' is missing. Please provide it in your configuration.");
            }

            // Assign: Link all registered interceptors from the service provider
            IEnumerable<ISaveChangesInterceptor> interceptors = sp.GetServices<ISaveChangesInterceptor>();
            options.AddInterceptors(interceptors);

            // Initialize: Configure Npgsql provider with auto-discovered migration assembly
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
            });

            // Initialize: Apply snake_case naming convention
            options.UseSnakeCaseNamingConvention();

            // Log: Enable diagnostic logging and error details in development environments only
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // Suppress: Ignore specific non-critical EF Core warnings
            options.ConfigureWarnings(warnings =>
            {
                warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
            });
        });

        #endregion

        #region Interface Mapping

        // Map: Map provided interface type to concrete context implementation
        builder.Services.AddScoped<TInterface>(sp => sp.GetRequiredService<TContext>());

        #endregion

        return builder;
    }

    #endregion
}