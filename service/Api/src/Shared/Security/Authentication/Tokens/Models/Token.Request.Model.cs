namespace Shared.Security.Authentication.Tokens.Models;

public record TokenRequestModel
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public string FullName { get; init; } = default!;
}

public record RevokeTokenRequestModel
{
    public string Token { get; init; } = default!;
    public string? Reason { get; init; } = null;
}