using Module.Identity.Features.Storefront.Passwords.Shared.Models;

namespace Module.Identity.Features.Storefront.Passwords.Change;

public static partial class ChangePassword
{
    public record Request : PasswordRequest
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
    }
}