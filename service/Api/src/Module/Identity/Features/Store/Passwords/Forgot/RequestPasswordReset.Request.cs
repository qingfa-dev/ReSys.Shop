namespace Module.Identity.Features.Store.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    // ============ REQUEST ============
    /// <summary>
    /// Request to receive a password reset link via email.
    /// </summary>
    /// <param name="Email">The email address to send the reset link to.</param>
    public record Request(string Email);
}
