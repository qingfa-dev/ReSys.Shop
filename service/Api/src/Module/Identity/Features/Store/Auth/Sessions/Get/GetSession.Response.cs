namespace Module.Identity.Features.Store.Auth.Sessions.Get;

public static partial class GetSession
{
    // ============ RESPONSE ============
    public record Response
    {
        public Guid Id { get; set; }
        public string[]? Roles { get; set; }
        public string[]? Permissions { get; set; }
    }

}