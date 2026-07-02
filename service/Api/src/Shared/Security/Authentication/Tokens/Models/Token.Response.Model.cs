namespace Shared.Security.Authentication.Tokens.Models;

public record TokenResponseModel(string Token, long ExpiresIn);
public record RefreshTokenResponseModel(
    Guid Id,
    string Token,
    Guid UserId,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string? RevokedReason,
    string? ReplacedByToken,
    bool IsActive);
