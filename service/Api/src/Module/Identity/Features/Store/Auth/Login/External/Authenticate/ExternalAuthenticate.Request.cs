using Module.Identity.Features.Store.Auth.Login.External.Shared.Models;

namespace Module.Identity.Features.Store.Auth.Login.External.Authenticate;

public static partial class ExternalAuthenticate
{
    public sealed record Request : BaseExternalLoginRequest;
}