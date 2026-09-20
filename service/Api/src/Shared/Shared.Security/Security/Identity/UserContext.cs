using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using Shared.Application.Models.Results;

namespace Shared.Security.Identity;

public sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Result<Guid?> CurrentUserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out Guid id))
                return new Result<Guid?>(isSuccess: true, value: id);
            return new Result<Guid?>(isSuccess: true, value: null);
        }
    }

    public Result<bool> IsAuthenticated
    {
        get
        {
            return new Result<bool>(
                isSuccess: true,
                value: _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true);
        }
    }
}
