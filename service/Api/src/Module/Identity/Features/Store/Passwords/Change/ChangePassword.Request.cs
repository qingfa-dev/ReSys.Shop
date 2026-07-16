using Module.Identity.Features.Store.Passwords.Shared.Models;

namespace Module.Identity.Features.Store.Passwords.Change;

public static partial class ChangePassword
{
    public record Request : PasswordRequest
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
    }
}