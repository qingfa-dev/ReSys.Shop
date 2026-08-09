using Module.Identity.Features.Shared.Storefront.Passwords.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    public record Request : PasswordRequest;
}