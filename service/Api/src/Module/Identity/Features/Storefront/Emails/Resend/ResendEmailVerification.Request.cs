using Module.Identity.Features.Storefront.Emails.Shared.Models;

namespace Module.Identity.Features.Storefront.Emails.Resend;

public static partial class ResendEmailVerification
{
    public record Request : EmailRequest;
}