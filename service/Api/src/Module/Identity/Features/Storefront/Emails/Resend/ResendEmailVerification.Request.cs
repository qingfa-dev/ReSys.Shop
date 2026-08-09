using Module.Identity.Features.Shared.Storefront.Emails.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Emails.Resend;

public static partial class ResendEmailVerification
{
    public record Request : EmailRequest;
}