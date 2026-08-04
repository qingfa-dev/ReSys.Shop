namespace Module.Identity.Features.Storefront.Emails.Shared.Models;

public record EmailDetailResponse : EmailParameters
{
    public string? Message { get; init; }
}
