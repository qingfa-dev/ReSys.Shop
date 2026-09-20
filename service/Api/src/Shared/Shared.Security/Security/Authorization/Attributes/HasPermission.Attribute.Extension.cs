using Microsoft.AspNetCore.Builder;

using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Attributes;

/// <summary>
/// Provides extension methods for fluent authorization configuration.
/// </summary>
public static class HasPermissionExtensions
{
    /// <summary>
    /// Adds a requirement that the user must have the specified permission to access the endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="permission">The required permission name.</param>
    /// <returns>The original builder for chaining.</returns>
    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder builder, string permission)
    {
        return builder.RequireAuthorization(new HasPermissionAttribute(permission));
    }

    /// <summary>
    /// Adds a requirement that the user must have the specified permission to access the endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="permission">The <see cref="PermissionMetadata"/> value object.</param>
    /// <returns>The original builder for chaining.</returns>
    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder builder, PermissionMetadata permission)
    {
        return builder.HasPermission(permission.Identifier);
    }
}
