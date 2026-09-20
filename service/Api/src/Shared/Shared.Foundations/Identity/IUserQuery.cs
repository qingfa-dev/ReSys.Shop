using System.Threading.Tasks;

using Shared.Application.Models.Results;

public interface IUserQuery
{
    Task<Result<Guid?>> GetUserIdByEmailAsync(string email);
    Task<Result<Guid?>> GetUserIdByUserNameAsync(string userName);
    Task<Result<Guid?>> GetUserIdByIdAsync(Guid id);
    Task<Result<bool>> IsInRoleAsync(Guid userId, string roleName);
}
