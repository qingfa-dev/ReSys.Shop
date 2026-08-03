using Module.Identity.Features.Storefront.Shared.Models;

namespace Module.Identity.Features.Storefront.Auth.Sessions.Refresh;

public static partial class RefreshSession
{
    public record Response : BaseTokenResponseModel;
}