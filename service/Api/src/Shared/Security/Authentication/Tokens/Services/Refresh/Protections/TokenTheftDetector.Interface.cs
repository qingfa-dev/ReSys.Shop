namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

public interface ITokenTheftDetector
{
    Task<Result<bool>> IsTokenReusedAsync(string token, Guid userId, CancellationToken ct = default);
    Task MarkTokenAsUsedAsync(string token, Guid userId, CancellationToken ct = default);
    Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default);
}
