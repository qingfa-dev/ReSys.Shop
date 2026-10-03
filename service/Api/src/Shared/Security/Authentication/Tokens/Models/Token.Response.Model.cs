namespace Shared.Security.Authentication.Tokens.Models;

public record TokenResponseModel
{
    public string Token { get; init; } = default!;
    public long ExpiresIn { get; init; }
}

public record RefreshTokenResponseModel
{
    public Guid Id { get; init; }
    public string Token { get; init; } = default!;
    public Guid UserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTime? RevokedAt { get; init; }
    public string? RevokedReason { get; init; }
    public string? ReplacedByToken { get; init; }
    public bool IsActive { get; init; }
}
