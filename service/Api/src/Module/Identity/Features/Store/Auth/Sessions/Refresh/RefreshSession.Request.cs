using Module.Identity.Features.Store.Shared.Models;

namespace Module.Identity.Features.Store.Auth.Sessions.Refresh;

public static partial class RefreshSession
{
    public record Request : BaseRefreshTokenRequestModel;
}
