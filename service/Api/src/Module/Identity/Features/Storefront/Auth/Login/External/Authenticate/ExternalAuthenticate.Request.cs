using Module.Identity.Features.Shared.Storefront.Auth.Login.External.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Auth.Login.External.Authenticate;

public static partial class ExternalAuthenticate
{
    public sealed record Request : BaseExternalLoginRequest;
}