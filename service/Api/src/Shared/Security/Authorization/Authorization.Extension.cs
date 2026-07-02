using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.Authorization.Options;
using Shared.Security.Authorization.Permissions.Caches;
using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Permissions.Store;
using Shared.Security.Authorization.Policies;
using Shared.Security.Authorization.Requirements;

namespace Shared.Security.Authorization;

// Boundary: DI Registration — Wires all authorization services into the DI container.

/// <summary>
/// Extension methods for registering authorization services.
/// </summary>
public static class AuthorizationExtensions
{
    #region Service Registration

    /// <summary>
    /// Registers all authorization services: options, cache, store, service,
    /// policy provider, and authorization handler.
    /// </summary>
    public static WebApplicationBuilder AddApplicationAuthorization(
        this WebApplicationBuilder builder)
    {
        #region Options Configuration

        // Validate: AuthzSetting is required and must pass FluentValidation rules
        builder.Services.AddSingleton<IValidator<AuthzSetting>, AuthzSettingValidator>();
        // Initialize: Fluent options builder for authorization settings bound to config
        builder.Services.AddOptions<AuthzSetting>()
            .BindConfiguration(AuthzSetting.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        #endregion

        #region Permission Backing Services

        // Cache: Permission lookups via ICacheService with tag-based invalidation
        builder.Services.AddSingleton<IPermissionCache, PermissionCache>();

        // Store: Persist role/user permission claims via EF Core
        builder.Services.AddScoped<IPermissionStore, PermissionStoreService>();

        // Service: Orchestrate cache-first permission resolution with role merging
        builder.Services.AddScoped<IPermissionService, PermissionService>();

        #endregion

        #region Authorization Pipeline

        // Policy: Resolve [HasPermission] attributes to AuthorizationPolicy on demand (Singleton; uses static PermissionContext.IsKnown)
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Handler: Evaluate PermissionRequirement via effective user permissions (admin bypass)
        builder.Services.AddScoped<IAuthorizationHandler, PermissionRequirementAuthorizationHandler>();

        // Enable: Core ASP.NET Core authorization services
        builder.Services.AddAuthorization();

        #endregion

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    /// <summary>
    /// Configures the application to use authorization middleware for policy enforcement.
    /// </summary>
    public static WebApplication UseApplicationAuthorization(this WebApplication app)
    {
        // Use: Authorization middleware to enforce access policies
        app.UseAuthorization();

        return app;
    }

    #endregion
}
