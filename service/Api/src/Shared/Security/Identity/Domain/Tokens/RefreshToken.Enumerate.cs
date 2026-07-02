namespace Shared.Security.Identity.Domain.Tokens;

public enum RefreshTokenRevocationReason
{
    Replaced = 1,
    UserLogout = 2,
    UserLogoutAll = 3,
    ReuseDetected = 4,
}