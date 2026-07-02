using Module.Identity.Features.Store.Shared.Models;

namespace Module.Identity.Features.Store.Auth.Password;

public static partial class PasswordLogin
{
    // Response
    public record Response : BaseTokenResponseModel;
}