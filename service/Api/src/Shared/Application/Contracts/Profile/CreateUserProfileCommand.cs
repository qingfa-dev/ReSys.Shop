using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Profile;

public sealed record CreateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string? LastName,
    string Email) : ICommand<CreateUserProfileResult>;

public sealed record CreateUserProfileResult(Guid ProfileId);
