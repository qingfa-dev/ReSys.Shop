using Module.Identity.Features.Admin.Users.Shared.Models;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Models.Optionals;
using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Shared.Mappings;

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

    private static Optional<string?> ToOptional(string? value) =>
        string.IsNullOrEmpty(value) ? Optional<string?>.None : value;

    public static Result<User> MapToDomain<T>(this T request, User user) where T : UserParameter
    {
        var updateResult = user.Update(
            userName: ToOptional(request.UserName),
            email: ToOptional(request.Email),
            firstName: ToOptional(request.FirstName),
            lastName: ToOptional(request.LastName),
            phoneNumber: string.IsNullOrEmpty(request.PhoneNumber) ? Optional<string?>.None : request.PhoneNumber,
            emailConfirmed: request.EmailConfirmed,
            phoneNumberConfirmed: request.PhoneNumberConfirmed);
        if (updateResult.IsFailure)
            return Result<User>.Validation(errors: updateResult.Errors);
        
        AuditableBehavior.Touch(entity: user, atUtc: DateTimeOffset.UtcNow);
        return user;
    }
}