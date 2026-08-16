using Module.Identity.Features.Admin.Shared.Models;
using Shared.Application.Domain.Concerns.Auditable;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Shared.Mappings;

public static partial class UserMapping
{
    public static Result<User> MapToDomain<T>(this T request) where T : UserParameter
    {
        var result = UserMethod.Create(
            userName: request.UserName,
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            phoneNumber: request.PhoneNumber,
            emailConfirmed: request.EmailConfirmed,
            phoneNumberConfirmed: request.PhoneNumberConfirmed);
        if (result.IsFailure)
            return Result<User>.Validation(errors: result.Errors);
        var entity = result.Value;

        AuditableBehavior.Create(entity: entity, atUtc: DateTimeOffset.UtcNow);
        return entity;
    }

    public static Result<User> MapToDomain<T>(this T request, User user) where T : UserParameter
    {
        var updateResult = user.Update(
            userName: string.IsNullOrEmpty(request.UserName) ? null : request.UserName,
            email: string.IsNullOrEmpty(request.Email) ? null : request.Email,
            firstName: string.IsNullOrEmpty(request.FirstName) ? null : request.FirstName,
            lastName: string.IsNullOrEmpty(request.LastName) ? null : request.LastName,
            phoneNumber: string.IsNullOrEmpty(request.PhoneNumber) ? null : request.PhoneNumber,
            emailConfirmed: request.EmailConfirmed,
            phoneNumberConfirmed: request.PhoneNumberConfirmed);
        if (updateResult.IsFailure)
            return Result<User>.Validation(errors: updateResult.Errors);

        AuditableBehavior.Touch(entity: user, atUtc: DateTimeOffset.UtcNow);
        return user;
    }
}

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
