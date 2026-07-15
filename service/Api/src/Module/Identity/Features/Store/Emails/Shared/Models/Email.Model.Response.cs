namespace Module.Identity.Features.Store.Emails.Shared.Models;

public record EmailDetailResponse : EmailParameters
{
    public string? Message { get; init; }
}
