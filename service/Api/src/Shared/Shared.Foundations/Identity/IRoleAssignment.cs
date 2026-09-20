using System.Threading.Tasks;

using Shared.Application.Models.Results;

public interface IRoleAssignment
{
    Task<Result> AssignRoleAsync(Guid userId, string roleName);
    Task<Result> RemoveRoleAsync(Guid userId, string roleName);
}
