using Module.Identity.Features.Store.Emails.Shared.Models;

namespace Module.Identity.Features.Store.Emails.Change;

public static partial class ChangeEmail
{
    public record Request : EmailRequest
    {
        public string NewEmail { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}