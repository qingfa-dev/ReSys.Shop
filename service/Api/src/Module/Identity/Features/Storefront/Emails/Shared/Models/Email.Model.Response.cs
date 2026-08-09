namespace Module.Identity.Features.Shared.Storefront.Emails.Shared.Models;

public record EmailDetailResponse : EmailParameters
{
    public string? Message { get; init; }
}
