namespace Module.Identity.Features.Store.Passwords.Shared.Models;

public abstract record PasswordParameters
{
    public string Email { get; init; } = string.Empty;
}
