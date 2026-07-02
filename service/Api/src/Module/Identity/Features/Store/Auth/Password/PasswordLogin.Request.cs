using Module.Identity.Features.Store.Shared.Models;

namespace Module.Identity.Features.Store.Auth.Password;

public static partial class PasswordLogin
{
    // Request
    public record Request : BasePasswordLoginRequest;
}