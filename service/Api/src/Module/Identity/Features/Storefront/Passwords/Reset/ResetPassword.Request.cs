using Module.Identity.Features.Shared.Storefront.Passwords.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Passwords.Reset;

public static partial class ResetPassword
{
    public record Request : PasswordRequest
    {
        public required Guid UserId { get; init; }
        public required string Token { get; init; }
        public required string NewPassword { get; init; }
    }
}