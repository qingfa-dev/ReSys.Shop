namespace Module.Identity.Features.Store.Emails.Shared.Models;

public abstract record EmailParameters
{
    public string Email { get; init; } = string.Empty;
}
