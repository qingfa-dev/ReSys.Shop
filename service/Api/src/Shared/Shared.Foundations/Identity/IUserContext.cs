using Shared.Application.Models.Results;

public interface IUserContext
{
    Result<Guid?> CurrentUserId { get; }
    Result<bool> IsAuthenticated { get; }
}
