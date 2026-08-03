namespace Module.Identity.Features.Storefront.Emails.Shared.Models;

public abstract record EmailParameters
{
    public string Email { get; init; } = string.Empty;
}
