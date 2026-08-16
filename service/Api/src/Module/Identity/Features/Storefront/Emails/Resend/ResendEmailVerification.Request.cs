using Module.Identity.Features.Storefront.Shared.Models;

namespace Module.Identity.Features.Shared.Storefront.Emails.Resend;

public static partial class ResendEmailVerification
{
    public record Request : EmailRequest;
}