namespace Module.Identity.Features.Storefront.Passwords.Shared.Models;

public abstract record PasswordParameters
{
    public string Email { get; init; } = string.Empty;
}
