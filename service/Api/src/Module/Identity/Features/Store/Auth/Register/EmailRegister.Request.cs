namespace Module.Identity.Features.Store.Auth.Register;

public static partial class EmailRegister
{
    public record Request(
        string Email,
        string UserName,
        string Password,
        string FirstName,
        string? LastName = null,
        string? Phone = null,
        bool AcceptTerm = true);
}