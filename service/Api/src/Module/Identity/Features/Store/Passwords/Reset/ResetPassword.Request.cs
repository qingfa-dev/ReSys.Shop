namespace Module.Identity.Features.Store.Passwords.Reset;

public static partial class ResetPassword
{
    // ============ REQUEST ============
    /// <summary>
    /// Request to finalise a password reset with the token from email.
    /// </summary>
    /// <param name="UserId">The user whose password to reset.</param>
    /// <param name="Token">The reset token received via email.</param>
    /// <param name="NewPassword">The new password to set.</param>
    public record Request(
        Guid UserId,
        string Token,
        string NewPassword);
}