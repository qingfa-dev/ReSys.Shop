namespace Module.Identity.Features.Shared.Storefront.Emails.Shared.Models;

public abstract record EmailParameters
{
    public string Email { get; init; } = string.Empty;
}
