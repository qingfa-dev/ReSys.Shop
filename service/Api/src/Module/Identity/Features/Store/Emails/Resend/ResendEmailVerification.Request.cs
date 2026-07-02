namespace Module.Identity.Features.Store.Emails.Resend;

public static partial class ResendEmailVerification
{
    // ============ REQUEST ============
    /// <summary>
    /// Request to resend the email verification link.
    /// </summary>
    /// <param name="Email">The email address to resend verification for.</param>
    public record Request(string Email);
}
