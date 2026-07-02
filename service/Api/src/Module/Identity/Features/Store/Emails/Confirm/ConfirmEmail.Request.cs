namespace Module.Identity.Features.Store.Emails.Confirm;

public static partial class ConfirmEmail
{
    // ============ REQUEST ============
    /// <summary>
    /// Request to confirm a user's email or finalise an email change.
    /// </summary>
    /// <param name="UserId">The user whose email to confirm.</param>
    /// <param name="Token">Base64-url-encoded confirmation token.</param>
    /// <param name="NewEmail">Set for email change confirmation; null for initial verification.</param>
    public record Request(Guid UserId, string Token, string? NewEmail);
}
