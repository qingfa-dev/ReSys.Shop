using Module.Identity.Features.Store.Shared.Models;

namespace Module.Identity.Features.Store.Auth.Logout;

public static partial class Logout
{
    public record Request : BaseLogOutRequest;
}