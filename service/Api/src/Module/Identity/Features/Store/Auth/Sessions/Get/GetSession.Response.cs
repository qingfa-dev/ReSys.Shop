namespace Module.Identity.Features.Store.Auth.Sessions.Get;

public static partial class GetSession
{
    // ============ RESPONSE ============
    public sealed record Response(Guid Id, string[]? Roles, string[]? Permissions);

}