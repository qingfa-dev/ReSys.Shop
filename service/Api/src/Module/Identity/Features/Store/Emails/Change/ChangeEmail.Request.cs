namespace Module.Identity.Features.Store.Emails.Change;

public static partial class ChangeEmail
{
    // ============ REQUEST ============
    /// <summary>
    /// Request to initiate an email change for the authenticated user.
    /// </summary>
    /// <param name="NewEmail">The target email address to change to.</param>
    /// <param name="Password">Current password for re-authentication.</param>
    public record Request(
        string NewEmail,
        string Password);
}