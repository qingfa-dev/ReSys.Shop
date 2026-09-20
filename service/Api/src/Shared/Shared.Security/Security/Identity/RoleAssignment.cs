using Microsoft.AspNetCore.Identity;

using Shared.Application.Models.Results;
using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Identity;

public sealed class RoleAssignment : IRoleAssignment
{
    private readonly UserManager<User> _userManager;

    public RoleAssignment(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> AssignRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(Error.NotFound(
                code: "RoleAssignment.UserNotFound",
                message: $"No user found with id '{userId}'."));

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return Result.Failure(Error.Unexpected(
                code: "RoleAssignment.Failed",
                message: $"Failed to assign role '{roleName}': {string.Join(", ", result.Errors)}"));

        return Result.Ok();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(Error.NotFound(
                code: "RoleAssignment.UserNotFound",
                message: $"No user found with id '{userId}'."));

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            return Result.Failure(Error.Unexpected(
                code: "RoleAssignment.Failed",
                message: $"Failed to remove role '{roleName}': {string.Join(", ", result.Errors)}"));

        return Result.Ok();
    }
}
