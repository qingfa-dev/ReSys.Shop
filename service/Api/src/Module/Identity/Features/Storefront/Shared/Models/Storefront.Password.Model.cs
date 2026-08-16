namespace Module.Identity.Features.Storefront.Shared.Models;

public abstract record PasswordParameters
{
    public string Email { get; init; } = string.Empty;
}

public record PasswordRequest : PasswordParameters;
