namespace Module.Identity.Features.Store.Auth.Register;

public static partial class EmailRegister
{
    public record Response
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}