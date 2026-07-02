using Module.Identity.Features.Store.Auth.External.Shared.Models;

namespace Module.Identity.Features.Store.Auth.External.Authenticate;

public static partial class ExternalAuthenticate
{
    public sealed record Request : BaseExternalLoginRequest;
}
