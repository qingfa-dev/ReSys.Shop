using Module.Identity.Features.Storefront.Passwords.Shared.Models;

namespace Module.Identity.Features.Storefront.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    public record Request : PasswordRequest;
}