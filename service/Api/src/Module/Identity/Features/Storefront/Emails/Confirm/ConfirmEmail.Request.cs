using Module.Identity.Features.Shared.Storefront.Emails.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Emails.Confirm;

public static partial class ConfirmEmail
{
    public record Request : EmailRequest
    {
        public Guid UserId { get; init; }
        public string Token { get; init; } = string.Empty;
        public string? NewEmail { get; init; }
    }
}