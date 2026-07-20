using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Profile;

public sealed record CreateUserProfileCommand : ICommand<CreateUserProfileResult>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = default!;
    public string? LastName { get; init; }
    public string Email { get; init; } = default!;
}

public sealed record CreateUserProfileResult
{
    public Guid ProfileId { get; init; }
}
