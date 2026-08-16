using Module.Identity.Features.Storefront.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Auth.Sessions.Refresh;

public static partial class RefreshSession
{
    public record Response : BaseTokenResponseModel;
}