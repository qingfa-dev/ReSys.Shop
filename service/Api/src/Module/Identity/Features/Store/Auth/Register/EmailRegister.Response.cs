namespace Module.Identity.Features.Store.Auth.Register;

public static partial class EmailRegister
{
    public sealed record Response(Guid UserId, string Email, string Message);
}