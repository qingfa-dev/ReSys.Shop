using Module.Identity.Features.Shared.Storefront.Shared.Models;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Storefront.Shared.Mappings;

/// <summary>
/// Maps a user composite (user, roles, and permissions) onto a session response model.
/// </summary>
public static class SessionMapping
{
    /// <summary>
    /// Maps the user composite onto a response deriving from <see cref="SessionResponseModel"/>.
    /// </summary>
    public static T MapToSessionResponse<T>(
        this (User User, string[] Roles, HashSet<string> Permissions) source)
        where T : SessionResponseModel, new()
        => new T
        {
            Id = source.User.Id,
            UserName = source.User.UserName ?? string.Empty,
            Email = source.User.Email ?? string.Empty,
            Roles = source.Roles,
            Permissions = source.Permissions.ToArray()
        };
}
