namespace Module.Identity.Features.Shared.Storefront.Auth.Shared.Models;

public abstract record RegisterParameters
{
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public bool AcceptTerm { get; init; } = true;
}

public record RegisterRequest : RegisterParameters;
