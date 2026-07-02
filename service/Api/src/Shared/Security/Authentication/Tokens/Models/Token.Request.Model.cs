namespace Shared.Security.Authentication.Tokens.Models;

public record TokenRequestModel(Guid UserId, string Email, string FullName);

public record RevokeTokenRequestModel(
    string Token,
    string? Reason = null);