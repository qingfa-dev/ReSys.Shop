using Module.Identity.Features.Storefront.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Passwords.Change;

public static partial class ChangePassword
{
    public record Request : PasswordRequest
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
    }
}