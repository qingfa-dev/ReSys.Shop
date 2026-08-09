using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.Shared.Mappings;

public static partial class UserMapping
{
    public static T MapToDetail<T>(this User user) where T : UserDetailResponse, new()
    {
        return new T
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            ModifiedAtUtc = user.ModifiedAtUtc,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        };
    }

    public static T MapToListItem<T>(this User user) where T : UserListResponse, new()
    {
        return new T
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        };
    }
}