using Microsoft.AspNetCore.Identity;

using Shared.Application.Models.Results;
using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Identity;

public sealed class UserQuery : IUserQuery
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public UserQuery(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<Guid?>> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<Guid?>.Failure(Error.NotFound(
                code: "UserQuery.UserNotFound",
                message: $"No user found with email '{email}'."));
        return new Result<Guid?>(isSuccess: true, value: user.Id);
    }

    public async Task<Result<Guid?>> GetUserIdByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
            return Result<Guid?>.Failure(Error.NotFound(
                code: "UserQuery.UserNotFound",
                message: $"No user found with username '{userName}'."));
        return new Result<Guid?>(isSuccess: true, value: user.Id);
    }

    public async Task<Result<Guid?>> GetUserIdByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return Result<Guid?>.Failure(Error.NotFound(
                code: "UserQuery.UserNotFound",
                message: $"No user found with id '{id}'."));
        return new Result<Guid?>(isSuccess: true, value: user.Id);
    }

    public async Task<Result<bool>> IsInRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<bool>.Failure(Error.NotFound(
                code: "UserQuery.UserNotFound",
                message: $"No user found with id '{userId}'."));
        return new Result<bool>(
            isSuccess: true,
            value: await _userManager.IsInRoleAsync(user, roleName));
    }
}
