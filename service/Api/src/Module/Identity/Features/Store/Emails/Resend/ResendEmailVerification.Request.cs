using Module.Identity.Features.Store.Emails.Shared.Models;

namespace Module.Identity.Features.Store.Emails.Resend;

public static partial class ResendEmailVerification
{
    public record Request : EmailRequest;
}