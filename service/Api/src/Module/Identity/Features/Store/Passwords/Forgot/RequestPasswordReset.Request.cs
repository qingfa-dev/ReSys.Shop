using Module.Identity.Features.Store.Passwords.Shared.Models;

namespace Module.Identity.Features.Store.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    public record Request : PasswordRequest;
}