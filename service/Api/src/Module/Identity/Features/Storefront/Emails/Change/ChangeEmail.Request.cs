using Module.Identity.Features.Shared.Storefront.Emails.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Emails.Change;

public static partial class ChangeEmail
{
    public record Request : EmailRequest
    {
        public string NewEmail { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}