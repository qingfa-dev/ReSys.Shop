namespace Module.Identity.Features.Store.Passwords.Change;

public static partial class ChangePassword
{
    // ============ REQUEST ============
    /// <summary>
    /// Request to change the authenticated user's password.
    /// </summary>
    /// <param name="CurrentPassword">Current password for re-authentication.</param>
    /// <param name="NewPassword">New password to set.</param>
    public record Request(
        string CurrentPassword,
        string NewPassword);
}
